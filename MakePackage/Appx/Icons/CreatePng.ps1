
Param(
    [parameter(mandatory)][string]$inputDir,
    [parameter(mandatory)][string]$outputDir
)

$inkscape = 'C:\Program Files\Inkscape\bin\inkscape.exe'
#$inputDir = "Sources-Blue"
#$outputDir = "Png-Blue"

if (!(Test-Path $outputDir))
{
    New-Item -Path . -Name $outputDir -ItemType Directory
}


# error to break
trap { break }

$ErrorActionPreference = "stop"


function Export-Png($svg, $png)
{
    & $inkscape --export-filename=$png --export-overwrite $svg
}

<#
function Copy-TargetSizeLightUnplated($name)
{
    $targetSize = @(256, 48, 32, 24, 16)
    foreach($size in $targetSize)
    {
        Copy-Item "$name.altform-unplated_targetsize-$size.png" "$name.altform-lightunplated_targetsize-$size.png"
    }
}
#>


$files = (Get-ChildItem $inputDir\*.svg).Name

foreach($file in $files)
{
    $png = [System.IO.Path]::ChangeExtension($file,".png")
    Export-Png $inputDir\$file $outputDir\$png
}

#Copy-TargetSizeLightUnplated "AppList"



