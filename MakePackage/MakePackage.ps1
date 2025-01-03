﻿# パッケージ生成スクリプト
#
# 使用ツール：
#   - Wix Toolset
#   - pandoc

Param(
	[ValidateSet("All", "Zip", "Installer", "Appx", "Canary", "Beta", "Dev")]$Target = "All",

	# ビルドをスキップする
	[switch]$continue,

	# ログ出力のあるパッケージ作成
	[switch]$trace,

	# ターゲットx86
	[switch]$x86,

	# MSI作成時にMainComponents.wsxを更新する
	[switch]$updateComponent,

	# Postfix. Canary や Beta での番号重複回避用
	[string]$versionPostfix = ""
)

# error to break
trap { break }

$ErrorActionPreference = "stop"

$updateBuildVersion = $false
if ($Target -ne "Dev")
{
	$tChoiceDescription = "System.Management.Automation.Host.ChoiceDescription"
	$options = @(
		New-Object $tChoiceDescription ("&Yes", "Update version")
		New-Object $tChoiceDescription ("&No", "Keep version")
	)

	$result = $host.ui.PromptForChoice("Update version", "Increment build version after build?", $options, 0)
	switch ($result)
	{
		0 { $updateBuildVersion = $true; break; }
		1 { $updateBuildVersion = $false; break; }
	}
}

Write-Host
Write-Host "[Properties] ..." -fore Cyan
Write-Host "Target: $Target"
Write-Host "Continue: $continue"
Write-Host "Trace: $trace"
Write-Host "X86: $x86"
Write-Host "UpdateComponent: $updateComponent" 
Write-Host "VersionPostfix: $versionPostfix" 
Write-Host "UpdateBuildVersion: $updateBuildVersion" 
Write-Host
Read-Host "Press Enter to continue"

#
$product = 'NeeView'
$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64"

# sync current directory
[System.IO.Directory]::SetCurrentDirectory((Get-Location -PSProvider FileSystem).Path)

#---------------------
# get fileversion
function Get-FileVersion($fileName)
{
	throw "not supported."

	$major = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMajorPart
	$minor = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($fileName).FileMinorPart

	"$major.$minor"
}


#---------------------
# load vsersion
function Get-Version($projectFile)
{
	$xml = [xml](Get-Content $projectFile)

	$version = [String]$xml.Project.PropertyGroup.VersionPrefix

	if ($version -match '\d+\.\d+\.\d+')
	{
		return $version
	}
	
    throw "Cannot get version."
}

function Get-AppVersion($version)
{
	$tokens = $version.Split(".")
	if ($tokens.Length -ne 3)
	{
		throw "Wrong version format."
	}
	$tokens = $version.Split(".")
	$majorVersion = [int]$tokens[0]
	$minorVersion = [int]$tokens[1]
	return "$majorVersion.$minorVersion"
}

#---------------------
# save vsersion
function Set-Version($projectFile, $version)
{
	$xml = [xml](Get-Content $projectFile)

	$xml.Project.PropertyGroup.VersionPrefix = $version

	$xml.Save( $projectFile )
}

#---------------------
# increment version
function Add-Version($version)
{
	$tokens = $version.Split(".")
	if ($tokens.Length -ne 3)
	{
		throw "Wrong version format."
	}
	$tokens = $version.Split(".")
	$majorVersion = [int]$tokens[0]
	$minorVersion = [int]$tokens[1]
	$buildVersion = [int]$tokens[2] + 1
	return "$majorVersion.$minorVersion.$buildVersion"
}

#---------------------
# get git log
function Get-GitLog()
{
    $branch = Invoke-Expression "git rev-parse --abbrev-ref HEAD"
    $descrive = Invoke-Expression "git describe --abbrev=0 --tags"
	$date = Invoke-Expression 'git log -1 --pretty=format:"%ad" --date=iso'
	$result = Invoke-Expression "git log $descrive..head --encoding=Shift_JIS --pretty=format:`"%ae %s`""
	$result = $result | Where-Object {$_ -match "^nee.laboratory"} | ForEach-Object {$_ -replace "^[\w\.@]+ ",""}
	$result = $result | Where-Object { -not ($_ -match '^m.rge|^開発用|^作業中|\(dev\)|^-|^\.\.') } 
	$result = $result | ForEach-Object {$_ -replace "#(\d+)", "[#`$1](https://bitbucket.org/neelabo/neeview/issues/`$1)"}

    return "[${branch}] $descrive to head", $date, $result
}

#---------------------
# get git log (markdown)
function Get-GitLogMarkdown($title)
{
    $result = Get-GitLog
	$header = $result[0]
	$date = $result[1]
    $logs = $result[2]

	"## $title"
	"### $header"
	"Rev. $revision / $date"
	""
	$logs | ForEach-Object { "- $_" }
	""
	"This change log was generated automatically."
}

#--------------------
# replace keyword
function Replace-Content
{
	Param([string]$filepath, [string]$rep1, [string]$rep2)
	if ( $(Test-Path $filepath) -ne $True )
	{
		Write-Error "file not found"
		return
	}
	# input UTF8, output UTF8
	$file_contents = $(Get-Content -Encoding UTF8 $filepath) -replace $rep1, $rep2
	$file_contents | Out-File -Encoding UTF8 $filepath
}


#-----------------------
# variables
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Convert-Path "$scriptPath\.."
$solution = "$solutionDir\$product.sln"
$projectDir = "$solutionDir\$product"
$project = "$projectDir\$product.csproj"
$projectSusieDir = "$solutionDir\$product.Susie.Server"
$projectSusie = "$projectSusieDir\$product.Susie.Server.csproj"
#$projectTerminateDir = "$solutionDir\$product.Terminator"
#$ptojectTerminate = "$projectTerminateDir\$product.Terminator.csproj"
$projectProps = "$solutionDir\$product.props"

#----------------------
# build
function Build-Project($platform, $outputDir, $options)
{
	$defaultOptions = @(
		"-p:PublishProfile=FolderProfile-$platform.pubxml"
		"-c", "Release"
	)

	switch ($Target)
	{
		"Dev" { $defaultOptions += "-p:VersionSuffix=dev-${dateVersion}"; break; }
		"Canary" { $defaultOptions += "-p:VersionSuffix=canary-${dateVersion}"; break; }
		"Beta" { $defaultOptions += "-p:VersionSuffix=beta-${dateVersion}"; break; }
	}

	& dotnet publish $project $defaultOptions $options -o Publish\$outputDir
	if ($? -ne $true)
	{
		throw "build error"
	}

	& dotnet publish $projectSusie $defaultOptions $options -o Publish\$outputDir\Libraries\Susie
	if ($? -ne $true)
	{
		throw "build error"
	}
	
	#& dotnet publish $ptojectTerminate -p:PublishProfile=FolderProfile-$platform.pubxml -c Release
}

function Build-ProjectSelfContained($platform)
{
	$options = @(
		"--self-contained", "true"
	)

	Build-Project $platform "$product-$platform" $options
}

function Build-ProjectFrameworkDependent($platform)
{
	$options = @(
		"-p:PublishTrimmed=false"
		"--self-contained", "false"
	)

	Build-Project $platform "$product-$platform-fd" $options
}

#----------------------
# package section
function New-Package($platform, $productName, $productDir, $packageDir)
{
	$temp = New-Item $packageDir -ItemType Directory

	Copy-Item $productDir\* $packageDir -Recurse -Exclude ("*.pdb", "$product..settings.json")

	# fix native dll
	if ($platform -eq "x86")
	{
		#Write-Host Remove native x64
		Remove-Item $packageDir\Libraries\x64 -Recurse
	}
	if ($platform -eq "x64")
	{
		#Write-Host Remove native x86
		Remove-Item $packageDir\Libraries\x86 -Recurse
	}

	# custom config
	New-ConfigForZip $productDir "$productName.settings.json" $packageDir

	# generate README.html
	New-Readme $packageDir "en-us" "Zip"
	New-Readme $packageDir "ja-jp" "Zip"
}

#----------------------
# generate README.html
function New-Readme($packageDir, $culture, $target)
{
	$readmeSource = "Readme\$culture"

	$readmeDir = $packageDir + "\readme.$culture"

	$temp = New-Item $readmeDir -ItemType Directory 

	Copy-Item "$readmeSource\Overview.md" $readmeDir
	Copy-Item "$readmeSource\Canary.md" $readmeDir
	Copy-Item "$readmeSource\Environment.md" $readmeDir
	Copy-Item "$readmeSource\Contact.md" $readmeDir

	Copy-Item "$solutionDir\LICENSE.md" $readmeDir
	Copy-Item "$solutionDir\LICENSE.ja-jp.md" $readmeDir
	Copy-Item "$solutionDir\THIRDPARTY_LICENSES.md" $readmeDir
	Copy-Item "$solutionDir\NeeLaboratory.IO.Search\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"

	if ($target -eq "Canary")
	{
		Get-GitLogMarkdown "$product <VERSION/> - ChangeLog" | Set-Content -Encoding UTF8 "$readmeDir\ChangeLog.md"
	}
	else
	{
		Copy-Item "$readmeSource\ChangeLog.md" $readmeDir
	}

	$postfix = $appVersion
	$announce = ""
	if ($target -eq "Canary")
	{
		$postfix = "Canary ${dateVersion}"
		$announce = "Rev. ${revision}`r`n`r`n" + (Get-Content -Path "$readmeDir/Canary.md" -Raw -Encoding UTF8)
	}

	# edit README.md
	Replace-Content "$readmeDir\Overview.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\Overview.md" "<ANNOUNCE/>" "$announce"
	Replace-Content "$readmeDir\Environment.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\Contact.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\ChangeLog.md" "<VERSION/>" "$postfix"

	$readmeHtml = "README.html"

	if (-not ($culture -eq "en-us"))
	{
		$readmeHtml = "README.$culture.html"
	}

	$inputs = @()
	$inputs += "$readmeDir\Overview.md"

	if ($target -ne "Appx")
	{
		$inputs += "$readmeDir\Environment.md"
	}

	$inputs += "$readmeDir\Contact.md"
	$inputs += "$readmeDir\LICENSE.md"

	if ($culture -eq "ja-jp")
	{
		$inputs += "$readmeDir\LICENSE.ja-jp.md"
	}

	$inputs += "$readmeDir\THIRDPARTY_LICENSES.md"
	$inputs += "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"
	$inputs += "$readmeDir\ChangeLog.md"

	$output = "$packageDir\$readmeHtml"
	$css = "Readme\Style.html"
	
	# markdown to html by pandoc
	pandoc -s -t html5 -o $output --metadata title="$product $postfix" -H $css $inputs
	if ($? -ne $true)
	{
		throw "pandoc error"
	}

	Remove-Item $readmeDir -Recurse
}

#--------------------------
# remove ZIP
function Remove-Zip($packageZip)
{
	if (Test-Path $packageZip)
	{
		Remove-Item $packageZip
	}
}

#--------------------------
# archive to ZIP
function New-Zip($referenceDir, $packageDir, $packageZip)
{
	Copy-Item $referenceDir $packageDir -Recurse
	Optimize-Package $packageDir
	Compress-Archive $packageDir -DestinationPath $packageZip
}

#--------------------------
# make config for zip
function New-ConfigForZip($inputDir, $config, $outputDir)
{
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Zip"
	$jsonObject.SelfContained = Test-Path("$inputDir\hostfxr.dll")
	$jsonObject.Watermark = $false
	$jsonObject.UseLocalApplicationData = $false
	$jsonObject.Revision = $revision
	$jsonObject.DateVersion = $dateVersion
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

#--------------------------
# make config for installer
function New-ConfigForMsi($inputDir, $config, $outputDir)
{
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Msi"
	$jsonObject.SelfContained = $true
	$jsonObject.Watermark = $false
	$jsonObject.UseLocalApplicationData = $true
	$jsonObject.Revision = $revision
	$jsonObject.DateVersion = $dateVersion
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}


#--------------------------
# make config for appx
function New-ConfigForAppx($inputDir, $config, $outputDir)
{
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = "Appx"
	$jsonObject.SelfContained = $true
	$jsonObject.Watermark = $false
	$jsonObject.UseLocalApplicationData = $true
	$jsonObject.Revision = $revision
	$jsonObject.DateVersion = $dateVersion
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

#--------------------------
# make config for canary / beta
function New-ConfigForDevPackage($inputDir, $config, $target, $outputDir)
{
	$jsonObject = (Get-Content "$inputDir\$config" | ConvertFrom-Json)

	$jsonObject.PackageType = $target
	$jsonObject.SelfContained = Test-Path("$inputDir\hostfxr.dll")
	$jsonObject.Watermark = $true
	$jsonObject.UseLocalApplicationData = $false
	$jsonObject.Revision = $revision
	$jsonObject.DateVersion = $dateVersion
	$jsonObject.PathProcessGroup = $true
	$jsonObject.LogFile = $trace ? "TraceLog.txt" : $null

	$outputFile = Join-Path (Convert-Path $outputDir) $config
	ConvertTo-Json $jsonObject | Out-File $outputFile
}

#---------------------------
#
function New-EmptyFolder($dir)
{
	# remove folder
	if (Test-Path $dir)
	{
		Remove-Item $dir -Recurse
		Start-Sleep -m 100
	}

	# make folder
	$temp = New-Item $dir -ItemType Directory
}

#---------------------------
#
function New-PackageAppend($packageDir, $packageAppendDir)
{
	New-EmptyFolder $packageAppendDir

	# configure customize
	New-ConfigForMsi $packageDir "${product}.settings.json" $packageAppendDir

	# icons
	Copy-Item "$projectDir\Resources\App.ico" $packageAppendDir
}



#--------------------------
# remove Msi
function Remove-Msi($packageAppendDir, $packageMsi)
{
	if (Test-Path $packageMsi)
	{
		Remove-Item $packageMsi
	}

	if (Test-Path $packageAppxDir_x64)
	{
		Remove-Item $packageAppxDir_x64 -Recurse
	}
}

#--------------------------
# Msi
function New-Msi($arch, $packageDir, $packageAppendDir, $packageMsi)
{
	$candle = $env:WIX + 'bin\candle.exe'
	$light = $env:WIX + 'bin\light.exe'
	$heat = $env:WIX + 'bin\heat.exe'
	$torch = $env:WIX + 'bin\torch.exe'
	$wisubstg = "$Win10SDK\wisubstg.vbs"
	$wilangid = "$Win10SDK\wilangid.vbs"

	$1041Msi = "$packageAppendDir\1041.msi"
	$1041Mst = "$packageAppendDir\1041.mst"

	#-------------------------
	# WiX
	#-------------------------

	$ErrorActionPreference = "stop"

	function New-MainComponents
	{
		$wxs = Convert-Path "WixSource\$arch\MainComponents.wxs"
		& $heat dir "$packageDir" -cg MainComponents -ag -pog:Binaries -sfrag -srd -sreg -var var.ContentDir -dr INSTALLFOLDER -out $wxs
		if ($? -ne $true)
		{
			throw "heat error"
		}

		[xml]$xml = Get-Content $wxs

		# remove $product.exe
		$node = $xml.Wix.Fragment[0].DirectoryRef.Component | Where-Object{$_.File.Source -match "$product\.exe"}
		if ($null -ne $node)
		{
			$componentId = $node.Id
			$xml.Wix.Fragment[0].DirectoryRef.RemoveChild($node)

			$node = $xml.Wix.Fragment[1].ComponentGroup.ComponentRef | Where-Object{$_.Id -eq $componentId}
			$xml.Wix.Fragment[1].ComponentGroup.RemoveChild($node)
		}

		# remove $product.settings.json
		$node = $xml.Wix.Fragment[0].DirectoryRef.Component | Where-Object{$_.File.Source -match "$product\.settings\.json"}
		if ($null -ne $node)
		{
			$componentId = $node.Id
			$xml.Wix.Fragment[0].DirectoryRef.RemoveChild($node)

			$node = $xml.Wix.Fragment[1].ComponentGroup.ComponentRef | Where-Object{$_.Id -eq $componentId}
			$xml.Wix.Fragment[1].ComponentGroup.RemoveChild($node)
		}

		$xml.Save($wxs)
	}

	function New-MsiSub($packageMsi, $culture)
	{
		Write-Host "$packageMsi : $culture" -fore Cyan
		
		$wixObjDir = "$packageAppendDir\obj.$culture"
		New-EmptyFolder $wixObjDir

		& $candle -arch $arch -d"Platform=$arch" -d"AppVersion=$appVersion" -d"ProductVersion=$version" -d"ContentDir=$packageDir\\" -d"AppendDir=$packageDir.append\\" -d"LibrariesDir=$packageDir\\Libraries" -d"culture=$culture" -ext WixNetFxExtension -out "$wixObjDir\\"  WixSource\*.wxs .\WixSource\$arch\*.wxs
		if ($? -ne $true)
		{
			throw "candle error"
		}

		& $light -out "$packageMsi" -ext WixUIExtension -ext WixNetFxExtension -cultures:$culture -loc WixSource\Language-$culture.wxl  "$wixObjDir\*.wixobj"
		if ($? -ne $true)
		{
			throw "light error" 
		}
	}

	## Create MainComponents.wxs
	if ($updateComponent)
	{
		Write-Host "Create MainComponents.wsx`n" -fore Cyan
		New-MainComponents
	}

	New-MsiSub $packageMsi "en-us"
	New-MsiSub $1041Msi "ja-jp"

	& $torch -p -t language $packageMsi $1041Msi -out $1041Mst
	if ($? -ne $true)
	{
		throw "torch error"
	}

	#-------------------------
	# WinSDK
	#-------------------------

	& cscript "$wisubstg" "$packageMsi" $1041Mst 1041
	if ($? -ne $true)
	{
		throw "wisubstg.vbs error"
	}

	& cscript "$wilangid" "$packageMsi" Package 1033,1041
	if ($? -ne $true)
	{
		throw "wilangid.vbs error"
	}
}


#--------------------------
# Appx remove
function Remove-Appx($packageAppendDir, $appx)
{
	if (Test-Path $appx)
	{
		Remove-Item $appx
	}

	if (Test-Path $packageAppxDir_x64)
	{
		Remove-Item $packageAppxDir_x64 -Recurse
	}
}

#--------------------------
# Appx 
function New-Appx($arch, $packageDir, $packageAppendDir, $appx)
{
	$packgaeFilesDir = "$packageAppendDir/PackageFiles"
	$contentDir = "$packgaeFilesDir/$product"

	# copy package base files
	Copy-Item "Appx\Resources" $packgaeFilesDir -Recurse -Force

	# copy resources.pri
	Copy-Item "Appx\resources.pri" $packgaeFilesDir

	# update assembly
	Copy-Item $packageDir $contentDir -Recurse -Force
	New-ConfigForAppx $packageDir "${product}.settings.json" $contentDir

	# generate README.html
	New-Readme $contentDir "en-us" "Appx"
	New-Readme $contentDir "ja-jp" "Appx"

	. $env:CersPath/_$product.Parameter.ps1
	$param = Get-AppxParameter
	$appxName = $param.name
	$appxPublisher = $param.publisher

	# generate AppManifest
	$content = Get-Content "Appx\AppxManifest.xml"
	$content = $content -replace "%NAME%","$appxName"
	$content = $content -replace "%PUBLISHER%","$appxPublisher"
	$content = $content -replace "%VERSION%","$assemblyVersion"
	$content = $content -replace "%ARCH%", "$arch"
	$content | Out-File -Encoding UTF8 "$packgaeFilesDir\AppxManifest.xml"

	# re-package
	& "$Win10SDK\makeappx.exe" pack /l /d "$packgaeFilesDir" /p "$appx"
	if ($? -ne $true)
	{
		throw "makeappx.exe error"
	}

	# signing
	& "$Win10SDK\signtool.exe" sign -f "$env:CersPath/_$product.pfx" -fd SHA256 -v "$appx"
	if ($? -ne $true)
	{
		throw "signtool.exe error"
	}
}


#--------------------------
# archive to Canary.ZIP
function Remove-Canary()
{
	if (Test-Path $packageCanary)
	{
		Remove-Item $packageCanary
	}

	if (Test-Path $packageCanaryDir)
	{
		Remove-Item $packageCanaryDir -Recurse
	}
}

function New-Canary($packageDir)
{
	New-DevPackage $packageDir $packageCanaryDir $packageCanary "Canary"
}

function New-CanaryAnyCPU($packageDir)
{
	New-DevPackage $packageDir $packageCanaryDir_AnyCPU $packageCanary_AnyCPU "Canary"
}

#--------------------------
# archive to Beta.ZIP
function Remove-Beta()
{
	if (Test-Path $packageBeta)
	{
		Remove-Item $packageBeta
	}

	if (Test-Path $packageBetaDir)
	{
		Remove-Item $packageBetaDir -Recurse
	}
}

function New-Beta($packageDir)
{
	New-DevPackage $packageDir $packageBetaDir $packageBeta "Beta"
}

#--------------------------
# archive to Canary/Beta.ZIP
function New-DevPackage($packageDir, $devPackageDir, $devPackage, $target)
{
	# update assembly
	Copy-Item $packageDir $devPackageDir -Recurse
	New-ConfigForDevPackage $packageDir "${product}.settings.json" $target $devPackageDir

	# generate README.html
	New-Readme $devPackageDir "en-us" $target
	New-Readme $devPackageDir "ja-jp" $target

	Optimize-Package $devPackageDir
	Compress-Archive $devPackageDir -DestinationPath $devPackage
}

#--------------------------
# Optimizing file placement with NetBeauty
# https://github.com/nulastudio/NetBeauty2
function Optimize-Package($packageDir)
{
	Write-Host "NetBeauty2" -fore Cyan
	nbeauty2 --usepatch --loglevel Detail $packageDir Libraries

	$unusedFile = "$packageDir\hostfxr.dll.bak"
	if (Test-Path $unusedFile)
	{
		Remove-Item $unusedFile
	}
}


$build_x64 = $false
$build_x86 = $false
$build_x64_fd = $false

#--------------------------
# remove build objects
function Remove-BuildObjects
{
	Get-ChildItem -Directory "$packagePrefix*" | Remove-Item -Recurse

	Get-ChildItem -File "$packagePrefix*.*" | Remove-Item

	if (Test-Path $publishDir)
	{
		Remove-Item $publishDir -Recurse
	}
	if (Test-Path $packageCanaryDir)
	{
		Remove-Item $packageCanaryDir -Recurse -Force
	}
	if (Test-Path $packageBetaDir)
	{
		Remove-Item $packageBetaDir -Recurse -Force
	}
	if (Test-Path $packageCanaryWild)
	{
		Remove-Item $packageCanaryWild
	}
	if (Test-Path $packageBetaWild)
	{
		Remove-Item $packageBetaWild
	}

	Start-Sleep -m 100
}

function Build-Clear
{
	# clear
	Write-Host "`n[Clear] ...`n" -fore Cyan
	Remove-BuildObjects
}

function Build-UpdateState
{
	$global:build_x64 = Test-Path $publishDir_x64
	$global:build_x86 = Test-Path $publishDir_x86
	$global:build_x64_fd = Test-Path $publishDir_x64_fd
}

function Build-PackageSorce-x64
{
	if ($global:build_x64 -eq $true) { return }

	# build
	Write-Host "`n[Build] ...`n" -fore Cyan
	Build-ProjectSelfContained "x64"
	
	# create package source
	Write-Host "`n[Package] ...`n" -fore Cyan
	New-Package "x64" $product $publishDir_x64 $packageDir_x64

	$global:build_x64 = $true
}

function Build-PackageSorce-x86
{
	if ($global:build_x86 -eq $true) { return }

	# build
	Write-Host "`n[Build x86] ...`n" -fore Cyan
	Build-ProjectSelfContained "x86"
	
	# create package source
	Write-Host "`n[Package x86] ...`n" -fore Cyan
	New-Package "x86" $product $publishDir_x86 $packageDir_x86

	$global:build_x86 = $true
}

function Build-PackageSorce-x64-fd
{
	if ($global:build_x64_fd -eq $true) { return }

	# build
	Write-Host "`n[Build framework dependent] ...`n" -fore Cyan
	Build-ProjectFrameworkDependent "x64"
	
	# create package source
	Write-Host "`n[Package framework dependent] ...`n" -fore Cyan
	New-Package "x64" $product $publishDir_x64_fd $packageDir_x64_fd

	$global:build_x64_fd = $true
}


function Build-Zip-x64
{
	Write-Host "`[Zip] ...`n" -fore Cyan

	Remove-Zip $packageZip_x64
	New-Zip $packageDir_x64 $packageName_x64 $packageZip_x64
	Write-Host "`nExport $packageZip_x64 successed.`n" -fore Green
}

function Build-Zip-x86
{
	Write-Host "`[Zip x86] ...`n" -fore Cyan

	Remove-Zip $packageZip_x86
	New-Zip $packageDir_x86 $packageName_x86 $packageZip_x86
	Write-Host "`nExport $packageZip_x86 successed.`n" -fore Green
}

function Build-Zip-x64-fd
{
	Write-Host "`[Zip fd] ...`n" -fore Cyan

	Remove-Zip $packageZip_x64_fd
	New-Zip $packageDir_x64_fd $packageName_x64_fd $packageZip_x64_fd
	Write-Host "`nExport $packageZip_x64_fd successed.`n" -fore Green
}


function Build-Installer-x64
{
	Write-Host "`n[Installer] ...`n" -fore Cyan
	
	Remove-Msi $packageAppendDir_x64 $packageMsi_x64
	New-PackageAppend $packageDir_x64 $packageAppendDir_x64
	New-Msi "x64" $packageDir_x64 $packageAppendDir_x64 $packageMsi_x64
	Write-Host "`nExport $packageMsi_x64 successed.`n" -fore Green
}

function Build-Installer-x86
{
	Write-Host "`n[Installer x86] ...`n" -fore Cyan

	Remove-Msi $packageAppendDir_x86 $packageMsi_x86
	New-PackageAppend $packageDir_x86 $packageAppendDir_x86
	New-Msi "x86" $packageDir_x86 $packageAppendDir_x86 $packageMsi_x86
	Write-Host "`nExport $packageMsi_x86 successed.`n" -fore Green
}

function Build-Appx-x64
{
	Write-Host "`n[Appx] ...`n" -fore Cyan

	if (Test-Path "$env:CersPath\_Parameter.ps1")
	{
		Remove-Appx $packageAppxDir_x64 $packageX64Appx
		New-Appx "x64" $packageDir_x64 $packageAppxDir_x64 $packageX64Appx
		Write-Host "`nExport $packageX64Appx successed.`n" -fore Green
	}
	else
	{
		Write-Host "`nWarning: not exist make appx envionment. skip!`n" -fore Yellow
	}
}

function Build-Appx-x86
{
	Write-Host "`n[Appx x86] ...`n" -fore Cyan

	if (Test-Path "$env:CersPath\_Parameter.ps1")
	{
		Remove-Appx $packageAppxDir_x86 $packageX86Appx
		New-Appx "x86" $packageDir_x86 $packageAppxDir_x86 $packageX86Appx
		Write-Host "`nExport $packageX86Appx successed.`n" -fore Green
	}
	else
	{
		Write-Host "`nWarning: not exist make appx envionment. skip!`n" -fore Yellow
	}
}

function Build-Canary
{
	Write-Host "`n[Canary] ...`n" -fore Cyan
	Remove-Canary
	New-Canary $packageDir_x64_fd
	Write-Host "`nExport $packageCanary successed.`n" -fore Green
}

function Build-Beta
{
	Write-Host "`n[Beta] ...`n" -fore Cyan
	Remove-Beta
	New-Beta $packageDir_x64
	Write-Host "`nExport $packageBeta successed.`n" -fore Green
}


function Export-Current
{
	Write-Host "`n[Current] ...`n" -fore Cyan
	if (Test-Path $packageDir_x64_fd)
	{
		if (-not (Test-Path $product))
		{
			New-Item $product -ItemType Directory
		}
		Copy-Item "$packageDir_x64_fd\*" "$product\" -Recurse -Force
	}
	else
	{
		Write-Host "`nWarning: not exist $packageDir_x64_fd. skip!`n" -fore Yellow
	}
}

#======================
# main
#======================

# versions
$version = Get-Version $projectProps
$appVersion = Get-AppVersion $version
$assemblyVersion = "$version.0"
$revision = (& git rev-parse --short HEAD).ToString()
$dateVersion = (Get-Date).ToString("MMdd") + $versionPostfix

$publishDir = "Publish"
$publishDir_x64 = "$publishDir\$product-x64"
$publishDir_x86 = "$publishDir\$product-x86"
$publishDir_x64_fd = "$publishDir\$product-x64-fd"
$packagePrefix = "$product$appVersion"
$packageDir_x64 = "$product$appVersion-x64"
$packageDir_x86 = "$product$appVersion-x86"
$packageDir_x64_fd = "$product$appVersion-x64-fd"
$packageAppendDir_x64 = "$packageDir_x64.append"
$packageAppendDir_x86 = "$packageDir_x86.append"
$packageName_x64 = "${product}${appVersion}"
$packageName_x86 = "${product}${appVersion}-x86"
$packageName_x64_fd = "${product}${appVersion}-fd"
$packageZip_x64 = "$packageName_x64.zip"
$packageZip_x86 = "$packageName_x86.zip"
$packageZip_x64_fd = "$packageName_x64_fd.zip"
$packageMsi_x64 = "$packageName_x64.msi"
$packageMsi_x86 = "$packageName_x86.msi"
$packageAppxDir_x64 = "${product}${appVersion}-appx-x64"
$packageAppxDir_x86 = "${product}${appVersion}-appx-x84"
$packageX86Appx = "${product}${appVersion}-x86.appx"
$packageX64Appx = "${product}${appVersion}.appx"
$packageCanaryDir = "${product}Canary"
$packageCanaryDir_AnyCPU = "${product}Canary-AnyCPU"
$packageCanary = "${product}Canary${dateVersion}.zip"
$packageCanary_AnyCPU = "${product}Canary${dateVersion}_AnyCPU.zip"
$packageCanaryWild = "${product}Canary*.zip"
$packageBetaDir = "${product}Beta"
$packageBeta = "${product}Beta${dateVersion}.zip"
$packageBetaWild = "${product}Beta*.zip"


if (-not $continue)
{
	Build-Clear
}

Build-UpdateState

if (($Target -eq "All") -or ($Target -eq "Zip"))
{
	if ($x86)
	{
		Build-PackageSorce-x86
		Build-Zip-x86
	}
	else
	{
		Build-PackageSorce-x64
		Build-Zip-x64

		Build-PackageSorce-x64-fd
		Build-Zip-x64-fd
	}
}

if (($Target -eq "All") -or ($Target -eq "Installer"))
{
	if ($x86)
	{
		Build-PackageSorce-x86
		Build-Installer-x86
	}
	else
	{
		Build-PackageSorce-x64
		Build-Installer-x64
	}
}

if (($Target -eq "All") -or ($Target -eq "Appx"))
{
	if ($x86)
	{
		Build-PackageSorce-x86
		Build-Appx-x86
	}
	else
	{
		Build-PackageSorce-x64
		Build-Appx-x64
	}
}

if (-not $x86)
{
	if ($Target -eq "Canary")
	{
		Build-PackageSorce-x64-fd
		Build-Canary
	}

	if ($Target -eq "Beta")
	{
		Build-PackageSorce-x64
		Build-Beta
	}

	if (-not $continue)
	{
		Build-PackageSorce-x64-fd
		Export-Current
	}
}

#--------------------------
# saev buid version
if ($updateBuildVersion)
{
	$nextVersion = Add-Version $version
	Write-Host "Update build version: $nextVersion"
	Set-Version $projectProps $nextVersion
}
else
{
	Write-Host "Keep build version."
}

#-------------------------
# Finish.
Write-Host "`nBuild $version All done.`n" -fore Green





