
$outputDir = "..\Resources\Assets"
#$outputDir = "Test"

# error to break
trap { break }

$ErrorActionPreference = "stop"

function Copy-RenameItem($item, $outputDir, $repSrc, $repDst)
{
    $newName = $item.Name.Replace($repSrc, $repDst)
    Copy-Item $item -Destination $outputDir\$newName
}

Get-ChildItem Png-Red\AppList.*.png | Copy-Item -Destination $outputDir
Get-ChildItem Png-Red\AppList.altform-unplated_targetsize-*.png | % {Copy-RenameItem $_ $outputDir "-unplated" "-lightunplated"}

Get-ChildItem Png-Blue\ImageLogo.*.png | Copy-Item -Destination $outputDir
Get-ChildItem Png-Blue\AppList.scale-*.png | % {Copy-RenameItem $_  $outputDir "AppList" "BookLogo"}
Get-ChildItem Png-Blue\AppList.targetsize-*.png | % {Copy-RenameItem $_  $outputDir "AppList" "BookLogo"}

