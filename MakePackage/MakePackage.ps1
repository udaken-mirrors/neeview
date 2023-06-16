# パッケージ生成スクリプト
#
# 使用ツール：
#   - Wix Toolset
#   - pandoc

Param(
	[ValidateSet("All", "Zip", "Installer", "Appx", "Canary", "Beta")]$Target = "All",
	[switch]$continue,
	[switch]$trace,
	[switch]$x86
)

# error to break
trap { break }

$ErrorActionPreference = "stop"

# MSI作成時にMainComponents.wsxを更新する?
$isCreateMainComponentsWxs = $true;

#
$product = 'NeeView'
$configuration = 'Release'
$framework = 'net6.0-windows'

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
	$result = $result | Where-Object { -not ($_ -match '^m.rge|^開発用|^作業中|\(dev\)|^-|^\.\.') } 

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
$project = "$projectDir\$product.csproj"
$projectSusieDir = "$solutionDir\NeeView.Susie.Server"
$projectSusie = "$projectSusieDir\NeeView.Susie.Server.csproj"
$projectTerminateDir = "$solutionDir\NeeView.Terminator"
$ptojectTerminate = "$projectTerminateDir\NeeView.Terminator.csproj"

#-----------------------
# procject output dir
function Get-ProjectOutputDir($projectDir, $platform)
{
	if ($platform -eq "AnyCPU")
	{
		"$projectDir\bin\$configuration\$framework"
	}
	else
	{
		"$projectDir\bin\$platform\$configuration\$framework"
	}
}

#----------------------
# build
function Build-Project($platform)
{
	& dotnet publish $project -p:PublishProfile=FolderProfile-$platform.pubxml -c Release
	if ($? -ne $true)
	{
		throw "build error"
	}

	& dotnet publish $projectSusie -p:PublishProfile=FolderProfile-$platform.pubxml -c Release
	if ($? -ne $true)
	{
		throw "build error"
	}
	
	#& dotnet publish $ptojectTerminate -p:PublishProfile=FolderProfile-$platform.pubxml -c Release
}

#----------------------
# package section
function New-Package($platform, $productName, $productDir, $publishSusieDir, $packageDir)
{
	$temp = New-Item $packageDir -ItemType Directory

	Copy-Item $productDir\* $packageDir -Recurse -Exclude ("*.pdb", "NeeView.dll.config")
	
	# fix native dll
	if ($platform -eq "x86")
	{
		Remove-Item $packageDir\x64 -Recurse
	}
	if ($platform -eq "x64")
	{
		Remove-Item $packageDir\x86 -Recurse
	}

	# custom config
	New-ConfigForZip $productDir "$productName.dll.config" $packageDir

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
	Copy-Item "$readmeSource\Environment.md" $readmeDir
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

	if ($target -ne ".appx")
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
	pandoc -s -t html5 -o $output --metadata title="NeeView $postfix" -H $css $inputs
	if ($? -ne $true)
	{
		throw "pandoc error"
	}

	Remove-Item $readmeDir -Recurse
}


#--------------------------
# archive to ZIP
function New-Zip($packageDir, $packageZip)
{
	Compress-Archive $packageDir -DestinationPath $packageZip
}


#--------------------------
function Get-CulturesFromConfig($inputDir, $config)
{
	[xml]$xml = Get-Content "$inputDir\$config"

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Cultures' } | Select -First 1
	return $add.value.Split(",")
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

	#$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	#$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion
	
	if ($trace)
	{
		#<add key="LogFile" value="TraceLog.txt" />
		$attribute1 = $xml.CreateAttribute('key')
		$attribute1.Value = 'LogFile';
		$attribute2 = $xml.CreateAttribute('value')
		$attribute2.Value = 'TraceLog.txt';
		$element = $xml.CreateElement('add');
		$element.Attributes.Append($attribute1);
		$element.Attributes.Append($attribute2);
		$xml.configuration.appSettings.AppendChild($element);
	}

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

	#$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	#$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion

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

	#$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	#$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion

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

	#$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'LibrariesPath' } | Select -First 1
	#$add.value = 'Libraries'

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'Revision' } | Select -First 1
	$add.value = $revision

	$add = $xml.configuration.appSettings.add | Where { $_.key -eq 'DateVersion' } | Select -First 1
	$add.value = $dateVersion

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
function New-PackageAppend($packageDir, $packageAppendDir)
{
	New-EmptyFolder $packageAppendDir

	# configure customize
	New-ConfigForMsi $packageDir "${product}.dll.config" $packageAppendDir

	# icons
	Copy-Item "$projectDir\Resources\App.ico" $packageAppendDir
}



#--------------------------
# WiX
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
		$wxs = "WixSource\$arch\MainComponents.wxs";
		& $heat dir "$packageDir" -cg MainComponents -ag -pog:Binaries -sfrag -srd -sreg -var var.ContentDir -dr INSTALLFOLDER -out $wxs
		if ($? -ne $true)
		{
			throw "heat error"
		}

		[xml]$xml = Get-Content $wxs

		# remove NeeView.exe
		$node = $xml.Wix.Fragment[0].DirectoryRef.Component | Where-Object{$_.File.Source -match "NeeView\.exe"}
		if ($null -ne $node)
		{
			$componentId = $node.Id
			$xml.Wix.Fragment[0].DirectoryRef.RemoveChild($node)

			$node = $xml.Wix.Fragment[1].ComponentGroup.ComponentRef | Where-Object{$_.Id -eq $componentId}
			$xml.Wix.Fragment[1].ComponentGroup.RemoveChild($node)
		}

		# remove NeeView.dll.config
		$node = $xml.Wix.Fragment[0].DirectoryRef.Component | Where-Object{$_.File.Source -match "NeeView\.dll\.config"}
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

		& $candle -arch $arch -d"Platform=$arch" -d"BuildVersion=$buildVersion" -d"ProductVersion=$version" -d"ContentDir=$packageDir\\" -d"AppendDir=$packageDir.append\\" -d"LibrariesDir=$packageDir\\Libraries" -d"culture=$culture" -ext WixNetFxExtension -out "$wixObjDir\\"  WixSource\*.wxs .\WixSource\$arch\*.wxs
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
	if ($isCreateMainComponentsWxs)
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
# Appx 
function New-Appx($arch, $packageDir, $packageAppendDir, $appx)
{
	$packgaeFilesDir = "$packageAppendDir/PackageFiles"
	$contentDir = "$packgaeFilesDir/NeeView"

	# copy package base files
	Copy-Item "Appx\Resources" $packgaeFilesDir -Recurse -Force

	# update assembly
	Copy-Item $packageDir $contentDir -Recurse -Force
	New-ConfigForAppx $packageDir "${product}.dll.config" $contentDir

	# generate README.html
	New-Readme $contentDir "en-us" ".appx"
	New-Readme $contentDir "ja-jp" ".appx"


	. $env:CersPath/_Parameter.ps1
	$param = Get-AppxParameter
	$appxName = $param.name
	$appxPublisher = $param.publisher

	# generate AppManifest
	$content = Get-Content "Appx\Resources\AppxManifest.xml"
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
	& "$Win10SDK\signtool.exe" sign -f "$env:CersPath/_neeview.pfx" -fd SHA256 -v "$appx"
	if ($? -ne $true)
	{
		throw "signtool.exe error"
	}
}


#--------------------------
# archive to Canary.ZIP
function New-Canary($packageDir)
{
	New-DevPackage $packageDir $packageCanaryDir $packageCanary ".canary"
}

function New-CanaryAnyCPU($packageDir)
{
	New-DevPackage $packageDir $packageCanaryDir_AnyCPU $packageCanary_AnyCPU ".canary"
}

#--------------------------
# archive to Beta.ZIP
function New-Beta($packageDir)
{
	New-DevPackage $packageDir $packageBetaDir $packageBeta ".beta"
}

#--------------------------
# archive to Canary/Beta.ZIP
function New-DevPackage($packageDir, $devPackageDir, $devPackage, $target)
{
	# update assembly
	Copy-Item $packageDir $devPackageDir -Recurse
	New-ConfigForDevPackage $packageDir "${product}.dll.config" $target $devPackageDir

	# generate README.html
	New-Readme $devPackageDir "en-us" $target
	New-Readme $devPackageDir "ja-jp" $target

	Compress-Archive $devPackageDir -DestinationPath $devPackage
}



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

function Build-PackageSorce-x64
{
	# build
	Write-Host "`n[Build] ...`n" -fore Cyan
	Build-Project "x64"
	
	# create package source
	Write-Host "`n[Package] ...`n" -fore Cyan
	New-Package "x64" $product $publishDir_x64 $publishSusieDir $packageDir_x64
}

function Build-PackageSorce-x86
{
	# build
	Write-Host "`n[Build x86] ...`n" -fore Cyan
	Build-Project "x86"
	
	# create package source
	Write-Host "`n[Package x86] ...`n" -fore Cyan
	New-Package "x86" $product $publishDir_x86 $publishSusieDir $packageDir_x86
}

function Build-Zip-x64
{
	Write-Host "`[Zip] ...`n" -fore Cyan

	New-Zip $packageDir_x64 $packageZip_x64
	Write-Host "`nExport $packageZip_x64 successed.`n" -fore Green
}

function Build-Zip-x86
{
	Write-Host "`[Zip x86] ...`n" -fore Cyan

	New-Zip $packageDir_x86 $packageZip_x86
	Write-Host "`nExport $packageZip_x86 successed.`n" -fore Green
}

function Build-Installer-x64
{
	Write-Host "`n[Installer] ...`n" -fore Cyan
	
	New-PackageAppend $packageDir_x64 $packageAppendDir_x64
	New-Msi "x64" $packageDir_x64 $packageAppendDir_x64 $packageMsi_x64
	Write-Host "`nExport $packageMsi_x64 successed.`n" -fore Green
}

function Build-Installer-x86
{
	Write-Host "`n[Installer x86] ...`n" -fore Cyan

	New-PackageAppend $packageDir_x86 $packageAppendDir_x86
	New-Msi "x86" $packageDir_x86 $packageAppendDir_x86 $packageMsi_x86
	Write-Host "`nExport $packageMsi_x86 successed.`n" -fore Green
}

function Build-Appx-x64
{
	Write-Host "`n[Appx] ...`n" -fore Cyan

	if (Test-Path "$env:CersPath\_Parameter.ps1")
	{
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
	New-Canary $packageDir_x64
	Write-Host "`nExport $packageCanary successed.`n" -fore Green
}

function Build-Beta
{
	Write-Host "`n[Beta] ...`n" -fore Cyan
	New-Beta $packageDir_x64
	Write-Host "`nExport $packageBeta successed.`n" -fore Green
}


function Export-Current
{
	Write-Host "`n[Current] ...`n" -fore Cyan
	if (Test-Path $packageDir_x64)
	{
		if (-not (Test-Path $product))
		{
			New-Item $product -ItemType Directory
		}
		Copy-Item "$packageDir_x64\*" "$product\" -Recurse -Force
	}
	else
	{
		Write-Host "`nWarning: not exist $packageDir_x64. skip!`n" -fore Yellow
	}
}


#======================
# main
#======================

# versions
$version = Get-Version $project
$buildCount = Get-BuildCount
$buildVersion = "$version.$buildCount"
$assemblyVersion = "$version.$buildCount.0"
$revision = (& git rev-parse --short HEAD).ToString()
$dateVersion = (Get-Date).ToString("MMdd")

$publishDir = "Publish"
$publishDir_x64 = "$publishDir\NeeView-x64"
$publishDir_x86 = "$publishDir\NeeView-x86"
$packagePrefix = "$product$version"
$packageDir_x64 = "$product$version-x64"
$packageDir_x86 = "$product$version-x86"
$packageAppendDir_x64 = "$packageDir_x64.append"
$packageAppendDir_x86 = "$packageDir_x86.append"
$packageZip_x64 = "${product}${version}-x64.zip"
$packageZip_x86 = "${product}${version}-x86.zip"
$packageMsi_x64 = "${product}${version}-x64.msi"
$packageMsi_x86 = "${product}${version}-x86.msi"
$packageAppxDir_x64 = "${product}${version}-appx-x64"
$packageAppxDir_x86 = "${product}${version}-appx-x84"
$packageX86Appx = "${product}${version}-x86.appx"
$packageX64Appx = "${product}${version}-x64.appx"
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
	if ($x86)
	{
		Build-PackageSorce-x86
	}
	else
	{
		Build-PackageSorce-x64
	}
}

if (($Target -eq "All") -or ($Target -eq "Zip") -or ($Target -eq "Canary") -or ($Target -eq "Beta"))
{
	if ($x86)
	{
		Build-Zip-x86
	}
	else
	{
		Build-Zip-x64
	}
}

if (($Target -eq "All") -or ($Target -eq "Installer"))
{
	if ($x86)
	{
		Build-Installer-x86
	}
	else
	{
		Build-Installer-x64
	}
}

if (($Target -eq "All") -or ($Target -eq "Appx"))
{
	if ($x86)
	{
		Build-Appx-x86
	}
	else
	{
		Build-Appx-x64
	}
}

if (-not $x86)
{
	if (($Target -eq "All") -or ($Target -eq "Canary"))
	{
		Build-Canary
	}

	if (($Target -eq "All") -or ($Target -eq "Beta"))
	{
		Build-Beta
	}

	Export-Current
}

#--------------------------
# saev buid version
Set-BuildCount $buildCount

#-------------------------
# Finish.
Write-Host "`nBuild $buildVersion All done.`n" -fore Green





