﻿# パッケージ生成スクリプト
#
# 使用ツール：
#   - Wix Toolset
#   - pandoc

Param(
	[ValidateSet("All", "Zip", "Installer", "Appx", "Canary", "Beta")]$Target = "All",
	[switch]$continue
)

# error to break
trap { break }

$ErrorActionPreference = "stop"


#
$product = 'NeeView'
$configuration = 'Release'
$framework = 'net472'

#
$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x64"


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
# get base vsersion
function Get-Version($projectFile)
{
	$xml = [xml](Get-Content $projectFile)
	$version = [String]$xml.Project.PropertyGroup.Version;
	if ($version -match '(\d+\.\d+)\.\d+')
	{
		return $Matches[1]
	}
	
    throw "Cannot get Version."
}


#---------------------
# get build count
function Get-BuildCount()
{
	# auto increment build version
	$xml = [xml](Get-Content "BuildCount.xml")
	return [int]$xml.build + 1
}

#---------------------
# set build count
function Set-BuildCount($buildCount)
{
	$xml = [xml](Get-Content "BuildCount.xml")
	$xml.build = [string]$buildCount
	$xml.Save("BuildCount.xml")
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
	$result = $result | Where-Object { -not ($_ -match '^m.rge|^開発用|\(dev\)|^-|^\.\.') } 

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
	"($date)"
	""
	$logs | ForEach-Object { "- $_" }
	""
	"This list of changes was auto generated."
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
$productDir = "$projectDir\bin\$configuration\$framework"
$project = "$projectDir\$product.csproj"

#----------------------
# build
function Build-Project($assemblyVersion)
{
	$platform = "Any CPU"

	$vswhere = "$solutionDir\Tools\vswhere.exe"

    $vspath = & $vswhere -property installationPath -latest
    $msbuild = "$vspath\MSBuild\Current\Bin\MSBuild.exe"
	& $msbuild $solution /p:Configuration=$configuration /p:Platform=$platform /t:Clean,Build
	if ($? -ne $true)
	{
		throw "build error"
	}
}




#----------------------
# package section
function New-Package($productName, $productDir, $packageDir)
{
	$packageLibraryDir = $packageDir + "\Libraries"

	# make package folder
	$temp = New-Item $packageDir -ItemType Directory
	$temp = New-Item $packageLibraryDir -ItemType Directory

	# copy
	Copy-Item "$productDir\$productName.exe" $packageDir
	Copy-Item "$productDir\*.dll" $packageLibraryDir

	# custom config
	New-ConfigForZip $productDir "$productName.exe.config" $packageDir

	# copy NeeView.Susie.Server
	Copy-Item "$productDir\NeeView.Susie.Server.exe" $packageLibraryDir
	Copy-Item "$productDir\NeeView.Susie.Server.exe.config" $packageLibraryDir

	# copy language dll
	$langs = "ja-JP","x64","x86"
	foreach($lang in $langs)
	{
		Copy-Item "$productDir\$lang" $packageLibraryDir -Recurse
	}

	# generate README.html
	New-Readme $packageDir "en-us" ".zip"
	New-Readme $packageDir "ja-jp" ".zip"
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
	Copy-Item "$readmeSource\Emvironment.md" $readmeDir
	Copy-Item "$readmeSource\Contact.md" $readmeDir

	Copy-Item "$solutionDir\LICENSE.md" $readmeDir
	Copy-Item "$solutionDir\LICENSE.ja-jp.md" $readmeDir
	Copy-Item "$solutionDir\THIRDPARTY_LICENSES.md" $readmeDir
	Copy-Item "$solutionDir\NeeLaboratory.IO.Search\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md"

	if ($target -eq ".canary")
	{
		Get-GitLogMarkdown "NeeView <VERSION/> - ChangeLog" | Set-Content -Encoding UTF8 "$readmeDir\ChangeLog.md"
	}
	else
	{
		Copy-Item "$readmeSource\ChangeLog.md" $readmeDir
	}

	$postfix = $version
	$announce = ""
	if ($target -eq ".canary")
	{
		$postfix = "Canary"
		$announce = Get-Content -Path "$readmeDir/Canary.md" -Raw -Encoding UTF8
	}

	# edit README.md
	Replace-Content "$readmeDir\Overview.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\Overview.md" "<ANNOUNCE/>" "$announce"
	Replace-Content "$readmeDir\Emvironment.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\Contact.md" "<VERSION/>" "$postfix"
	Replace-Content "$readmeDir\ChangeLog.md" "<VERSION/>" "$postfix"

	$readmeHtml = "README.html"
	$readmeEnvironment = ""
	$readmeLicenseAppendix = ""

	if (-not ($culture -eq "en-us"))
	{
		$readmeHtml = "README.$culture.html"
	}

	if ($culture -eq "ja-jp")
	{
		$readmeLicenseAppendix = """$readmeDir\LICENSE.ja-jp.md"""
	}

	if ($target -ne ".appx")
	{
		$readmeEnvironment = """$readmeDir\Emvironment.md"""
	}

	# markdown to html by pandoc
	pandoc -s -t html5 -o "$packageDir\$readmeHtml" -H "Readme\Style.html" "$readmeDir\Overview.md" $readmeEnvironment "$readmeDir\Contact.md" "$readmeDir\LICENSE.md" $readmeLicenseAppendix "$readmeDir\THIRDPARTY_LICENSES.md" "$readmeDir\NeeLaboratory.IO.Search_THIRDPARTY_LICENSES.md" "$readmeDir\ChangeLog.md"

	Remove-Item $readmeDir -Recurse
}


#--------------------------
# archive to ZIP
function New-Zip
{
	Compress-Archive $packageDir -DestinationPath $packageZip
}

#--------------------------
#
function New-ConfigForZip($inputDir, $config, $outputDir)
{
	# make config for zip
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = '.zip'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'False'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config

	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
}

#--------------------------
#
function New-ConfigForMsi($inputDir, $config, $outputDir)
{
	# make config for installer
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = '.msi'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'True'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config
	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
}


#--------------------------
#
function New-ConfigForAppx($inputDir, $config, $outputDir)
{
	# make config for appx
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = '.appx'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'True'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config

	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
}

#--------------------------
#
function New-ConfigForDevPackage($inputDir, $config, $target, $outputDir)
{
	# make config for canary
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'PackageType' } | Select -First 1
	$add.value = $target

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'UseLocalApplicationData' } | Select -First 1
	$add.value = 'False'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	$add.value = 'Libraries'

	$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)
	$outputFile = Join-Path (Convert-Path $outputDir) $config

	$sw = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithoutBom)
	$xml.Save( $sw )
	$sw.Close()
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
function New-PackageAppend($packageDir)
{
	#$config = "$product.exe.config"
	$packageAppendDir = $packageDir + ".append"
	New-EmptyFolder $packageAppendDir

	# configure customize
	New-ConfigForMsi $packageDir "${product}.exe.config" $packageAppendDir

	# icons
	Copy-Item "$projectDir\Resources\App.ico" $packageAppendDir
}



#--------------------------
# WiX
function New-Msi($packageDir, $packageMsi)
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

	function New-DllComponents
	{
		& $heat dir "$packageDir\Libraries" -cg DllComponents -ag -pog:Binaries -sfrag -sreg -var var.LibrariesDir -dr INSTALLFOLDER -out WixSource\DllComponents.wxs
		if ($? -ne $true)
		{
			throw "heat error"
		}
	}

	function New-MsiSub($packageMsi, $culture)
	{
		Write-Host "$packageMsi : $culture" -fore Cyan
		
		$wixObjDir = "$packageAppendDir\obj.$culture"
		New-EmptyFolder $wixObjDir

		& $candle -d"BuildVersion=$buildVersion" -d"ProductVersion=$version" -d"ContentDir=$packageDir\\" -d"AppendDir=$packageDir.append\\" -d"LibrariesDir=$packageDir\\Libraries" -d"culture=$culture" -ext WixNetFxExtension -out "$wixObjDir\\"  WixSource\*.wxs
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

	### Create DllComponents.wxs
	#New-DllComponents

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
# Appx ready
function New-AppxReady
{
	# update assembly
	Copy-Item $packageDir $packageAppxProduct -Recurse -Force
	New-ConfigForAppx $packageDir "${product}.exe.config" $packageAppxProduct

	# generate README.html
	New-Readme $packageAppxProduct "en-us" ".appx"
	New-Readme $packageAppxProduct "ja-jp" ".appx"

	# copy icons
	Copy-Item "Appx\Resources\Assets\*.png" "$packageAppxFiles\Assets\" 
}

#--------------------------
# Appx
function New-Appx($arch, $appx)
{
	. Appx/_Parameter.ps1
	$param = Get-AppxParameter
	$appxName = $param.name
	$appxPublisher = $param.publisher

	# generate AppManifest
	$content = Get-Content "Appx\Resources\AppxManifest.xml"
	$content = $content -replace "%NAME%","$appxName"
	$content = $content -replace "%PUBLISHER%","$appxPublisher"
	$content = $content -replace "%VERSION%","$assemblyVersion"
	$content = $content -replace "%ARCH%", "$arch"
	$content | Out-File -Encoding UTF8 "$packageAppxFiles\AppxManifest.xml"


	# re-package
	& "$Win10SDK\makeappx.exe" pack /l /d "$packageAppxFiles" /p "$appx"
	if ($? -ne $true)
	{
		throw "makeappx.exe error"
	}

	# signing
	& "$Win10SDK\signtool.exe" sign -f "Appx/_neeview.pfx" -fd SHA256 -v "$appx"
	if ($? -ne $true)
	{
		throw "signtool.exe error"
	}
}


#--------------------------
# archive to Canary.ZIP
function New-Canary
{
	New-DevPackage $packageCanaryDir $packageCanary ".canary"
}

#--------------------------
# archive to Beta.ZIP
function New-Beta
{
	New-DevPackage $packageBetaDir $packageBeta ".beta"
}

#--------------------------
# archive to Canary/Beta.ZIP
function New-DevPackage($devPackageDir, $devPackage, $target)
{
	# update assembly
	Copy-Item $packageDir $devPackageDir -Recurse
	New-ConfigForDevPackage $packageDir "${product}.exe.config" $target $devPackageDir

	# generate README.html
	New-Readme $devPackageDir "en-us" $target
	New-Readme $devPackageDir "ja-jp" $target

	Compress-Archive $devPackageDir -DestinationPath $devPackage
}



#--------------------------
# remove build objects
function Remove-BuildObjects
{
	if (Test-Path $packageDir)
	{
		Remove-Item $packageDir -Recurse -Force
	}
	if (Test-Path $packageAppendDir)
	{
		Remove-Item $packageAppendDir -Recurse -Force
	}
	if (Test-Path $packageCanaryDir)
	{
		Remove-Item $packageCanaryDir -Recurse -Force
	}
	if (Test-Path $packageBetaDir)
	{
		Remove-Item $packageBetaDir -Recurse -Force
	}
	if (Test-Path $packageZip)
	{
		Remove-Item $packageZip
	}
	if (Test-Path $packageMsi)
	{
		Remove-Item $packageMsi
	}
	if (Test-Path $packageWixpdb)
	{
		Remove-Item $packageWixpdb
	}
	if (Test-Path $packageAppxProduct)
	{
		Remove-Item $packageAppxProduct -Recurse -Force
	}
	if (Test-Path $packageX86Appx)
	{
		Remove-Item $packageX86Appx
	}
	if (Test-Path $packageX64Appx)
	{
		Remove-Item $packageX64Appx
	}
	if (Test-Path $packageCanary)
	{
		Remove-Item $packageCanary
	}
	if (Test-Path $packageBeta)
	{
		Remove-Item $packageBeta
	}

	Start-Sleep -m 100
}



#======================
# main
#======================

# versions
$version = Get-Version $project
$buildCount = Get-BuildCount
$buildVersion = "$version.$buildCount"
$assemblyVersion = "$version.$buildCount.0"

$packageDir = "$product$version"
$packageAppendDir = $packageDir + ".append"
$packageZip = "${product}${version}.zip"
$packageMsi = "${product}${version}.msi"
$packageWixpdb = "${product}${version}.wixpdb"
$packageAppxRoot = "Appx\$product"
$packageAppxFiles = "$packageAppxRoot\PackageFiles"
$packageAppxProduct = "$packageAppxRoot\PackageFiles\$product"
$packageX86Appx = "${product}${version}-x86.appx"
$packageX64Appx = "${product}${version}-x64.appx"
$packageCanaryDir = "${product}Canary"
$packageCanary = "${product}Canary.zip"
$packageBetaDir = "${product}Beta"
$packageBeta = "${product}Beta.zip"

if (-not $continue)
{
	# clear
	Write-Host "`n[Clear] ...`n" -fore Cyan
	Remove-BuildObjects
	
	# build
	Write-Host "`n[Build] ...`n" -fore Cyan
	Build-Project $assemblyVersion

	#
	Write-Host "`n[Package] ...`n" -fore Cyan
	New-Package $product $productDir $packageDir
}

#
if (($Target -eq "All") -or ($Target -eq "Zip") -or ($Target -eq "Canary") -or ($Target -eq "Beta"))
{
	Write-Host "`[Zip] ...`n" -fore Cyan
	New-Zip
	Write-Host "`nExport $packageZip successed.`n" -fore Green
}

if (($Target -eq "All") -or ($Target -eq "Installer"))
{
	Write-Host "`n[Installer] ...`n" -fore Cyan

	New-PackageAppend $packageDir
	New-Msi $packageDir $packageMsi

	Write-Host "`nExport $packageMsi successed.`n" -fore Green
}


if (($Target -eq "All") -or ($Target -eq "Appx"))
{
	Write-Host "`n[Appx] ...`n" -fore Cyan

	if ((Test-Path $packageAppxRoot) -and (Test-Path "Appx/_Parameter.ps1"))
	{
		New-AppxReady
		New-Appx "x64" $packageX64Appx
		Write-Host "`nExport $packageX64Appx successed.`n" -fore Green
		New-Appx "x86" $packageX86Appx
		Write-Host "`nExport $packageX86Appx successed.`n" -fore Green
	}
	else
	{
		Write-Host "`nWarning: not exist make appx envionment. skip!`n" -fore Yellow
	}
}

if (($Target -eq "All") -or ($Target -eq "Canary"))
{
	Write-Host "`n[Canary] ...`n" -fore Cyan
	New-Canary
	Write-Host "`nExport $packageCanary successed.`n" -fore Green
}

if (($Target -eq "All") -or ($Target -eq "Beta"))
{
	Write-Host "`n[Beta] ...`n" -fore Cyan
	New-Beta
	Write-Host "`nExport $packageBeta successed.`n" -fore Green
}

# current
Write-Host "`n[Current] ...`n" -fore Cyan
if (Test-Path $packageDir)
{
	if (-not (Test-Path $product))
	{
		New-Item $product -ItemType Directory
	}
	Copy-Item "$packageDir\*" "$product\" -Recurse -Force
}
else
{
	Write-Host "`nWarning: not exist$packageDir. skip!`n" -fore Yellow
}

#--------------------------
# saev buid version
Set-BuildCount $buildCount

#-------------------------
# Finish.
Write-Host "`nBuild $buildVersion All done.`n" -fore Green





