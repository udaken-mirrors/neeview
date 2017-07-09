$AppName = "NeeView"
$Appx = "$AppName.appx"
$PackageFiles = "Appx\$AppName\PackageFiles"

$Win10SDK = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.15063.0\x64"

# error to break
trap { break }
$ErrorActionPreference = "Stop"

## copy
##Copy-Item ..\NeeView\NeeView.exe Work\NeeView\PackageFiles\NeeView\NeeView.exe


## re-package
Write-Host "`[Package] ...`n" -fore Cyan
& "$Win10SDK\makeappx.exe" pack /l /d "$PackageFiles" /p "$Appx"
if ($? -ne $true)
{
	throw "makeappx.exe error"
}

# signing
Write-Host "`[Sign] ...`n" -fore Cyan
& "$Win10SDK\signtool.exe" sign -f "Appx\_my.pfx" -fd SHA256 -v "$Appx"
if ($? -ne $true)
{
	throw "signtool.exe error"
}