<#
.SYNOPSIS
Converting Language Files and Json

.DESCRIPTION
Converts between language files ([culture].restext) and Json. This is a convenience tool, not a required feature.

.EXAMPLE
> .\ConvertRestext.ps1
Convert language files to Language.json.

.EXAMPLE
> .\ConvertRestext.ps1 -Mode FromJson -Sort
Generate language files from Language.json.
-Sort by key if specified.

.PARAMETER Mode
Convert mode
    ToJson: Convert to Json file (default)
    FromJson: Convert Json file to language files

.PARAMETER JsonFile
Json file name. Default is Language.json

.PARAMETER Sort
Sort data by key

.PARAMETER Trim
For FromJson, trim empty data.

.PARAMETER Cultures
For FromJson, Specify the culture to be processed.
Specify cultures separated by commas. If not specified, all cultures are processed.
e.g., -Cultures en,ja

#>

param (
   	[ValidateSet("ToJson", "FromJson")]$Mode = "ToJson",
    [string]$JsonFile = "Language.json",
    [switch]$Sort,
    [switch]$Trim,
    [string[]]$Cultures = @()
)

if (Cultures.Length -ne 0)
{
    Write-Host Cultures: Cultures
}
$Filter = $Cultures


$defaultCulture = "en"

# error to break
trap { break }
$ErrorActionPreference = "stop"


function Get-Restext
{
    param ([string]$culture)
    $restext = "$culture.restext"
    Write-Host Read $restext
    $array = @()
    foreach($line in Get-Content $restext) 
    {
        $tokens = $line -split "=", 2
        $array += [PSCustomObject]@{
            Key = $tokens[0]
            Value = $tokens[1]
        }
    }
    return $array
}

function ConvertTo-RestextMap
{
    param ($array)
    $map = @{}
    foreach ($obj in $array)
    {
        $map[$obj.Key] = $obj.Value
    }
    return $map
}

function Add-RestextToRestextTable
{
    param([PSCustomObject]$table, [string]$culture)
    $array = Get-Restext $culture
    $map = ConvertTo-RestextMap $array
    foreach ($property in $table.psobject.Properties)
    {
        $key = $property.Name
        $value = $map.$key ?? $null
        $property.Value | Add-Member -MemberType NoteProperty -Name $culture -Value $value
    }
}

function ConvertTo-RestextFromRestextTable
{
    param ([PSCustomObject]$table, [string]$culture)
    $lines = @()
    foreach ($property in $table.psobject.Properties)
    {
        if ($Trim -and ($null -eq $property.Value.$culture))
        {
            continue 
        }
        $lines += $property.Name + "=" + $property.Value.$culture
    }
    return $lines
}

function Get-RestextCultures
{
    $cultures = Get-ChildItem *.restext | ForEach-Object {[System.IO.Path]::GetFileNameWithoutExtension($_.Name)}
    return $cultures
}

function Get-DefaultRestextTable
{
    param ([string]$culture)
    $array = Get-Restext $culture
    $table = [PSCustomObject]@{}
    foreach ($pair in $array)
    {
        $obj = [PSCustomObject]@{
            $culture = $pair.Value
        }
        $table | Add-Member -MemberType NoteProperty -Name $pair.Key -Value $obj
    }
    return $table
}

function Get-RestextTable
{
    param([string[]]$cultures)

    $table = Get-DefaultRestextTable $defaultCulture
    foreach ($culture in $cultures)
    {
        if ($culture -ne $defaultCulture)
        {
            Add-RestextToRestextTable $table $culture
        }
    }
    return $table
}

function Set-RestextTable
{
    param([PSCustomObject]$table, [string[]]$cultures)
    foreach ($culture in Get-FilteredCultures($cultures))
    {
        $restext = "$culture.restext"
        Write-Host Write $restext 
        ConvertTo-RestextFromRestextTable $table $culture | Set-Content $restext -Encoding utf8
    }
}

function Get-FilteredCultures
{
    param([string[]]$cultures)

    if ($Filter.Length -eq 0)
    {
        return $cultures
    }
    else
    {
        return $cultures | Where-Object { $Filter.Contains($_) }
    }
}


function Sort-RestextTable
{ 
    param([PSCustomObject]$table)
    $newTable = [PSCustomObject]@{}
    foreach ($property in $table.psobject.properties | Sort-Object -Property Name)
    {
        $key = $property.Name
        $value = $property.Value
        $newTable | Add-Member -MemberType NoteProperty -Name $key -Value $value
    }
    return $newTable
}

function Get-CulturesFromRestextTable
{
    param([PSCustomObject]$table)
    $cultures = @()
    foreach ($property in $table.psobject.properties)
    {
        foreach ($culture in $property.Value.psobject.properties.name)
        {
            if (-not $cultures.Contains($culture))
            {
                $cultures += $culture
            }
        }
    }
    return $cultures
}

# fix typo (no used)
function Get-ReplaceTypo
{
    $transform = [PSCustomObject]@{
        "Confrict" = "Conflict"
        "Deault" = "Default"
        "Inclide" = "Include"
        "Sceme" = "Scheme"
        "Javascript" = "JavaScript"
        "Playlsit" = "Playlist"
        "Rerset" = "Reset"
        "Lastest" = "Latest"
        "Vertival" = "Vertical"
        "Scipt" = "Script"
        "Manipurate" = "Manipulate"
        "Fodler" = "Folder"
        "Visibled" = "Visible"
        "Openbook" = "OpenBook"
    }

    param([string]$s)
    foreach ($pair in $transform.psobject.properties)
    {
        $s = $s -creplace $pair.Name, $pair.Value
    }
    return $s
}

# fix typo (table)
function Get-ReplaceTypoTable
{ 
    param([PSCustomObject]$table)
    $newTable = [PSCustomObject]@{}
    foreach ($property in $table.psobject.properties)
    {
        $key = Get-ReplaceTypo $property.Name
        $value = $property.Value
        $newTable | Add-Member -MemberType NoteProperty -Name $key -Value $value
    }
    return $newTable
}

#
# MAIN
#

if ($Mode -eq "FromJson")
{
    Write-Host Read $JsonFile
    $table = Get-Content $JsonFile | ConvertFrom-Json   
    if ($Sort)
    {
        $table = Sort-RestextTable $table
    }
    $cultures = Get-CulturesFromRestextTable $table
    Set-ResTextTable $table $cultures
}
else
{
    $cultures = Get-RestextCultures
    $table = Get-RestextTable $cultures
    if ($Sort)
    {
        $table = Sort-RestextTable $table
    }
    Write-Host Write $JsonFile
    $table | ConvertTo-Json | Set-Content $JsonFile -Encoding utf8
}

