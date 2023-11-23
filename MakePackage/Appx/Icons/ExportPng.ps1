$inkscape = 'C:\Program Files\Inkscape\bin\inkscape.exe'

# error to break
trap { break }

$ErrorActionPreference = "stop"


function Export-Png($svg)
{
    $png = [System.IO.Path]::ChangeExtension($svg,".png")
    & $inkscape --export-filename=$png --export-overwrite $svg
}

function Copy-TargetSizeLightUnplated($name)
{
    $targetSize = @(256, 48, 32, 24, 16)
    foreach($size in $targetSize)
    {
        Copy-Item "$name.altform-unplated_targetsize-$size.png" "$name.altform-lightunplated_targetsize-$size.png"
    }
}


$files = @(
    "AppList.altform-unplated_targetsize-16.svg",
    "AppList.altform-unplated_targetsize-24.svg",
    "AppList.altform-unplated_targetsize-32.svg",
    "AppList.altform-unplated_targetsize-48.svg",
    "AppList.altform-unplated_targetsize-256.svg",
    "AppList.scale-100.svg",
    "AppList.scale-125.svg",
    "AppList.scale-150.svg",
    "AppList.scale-200.svg",
    "AppList.scale-400.svg",
    "AppList.targetsize-16.svg",
    "AppList.targetsize-24.svg",
    "AppList.targetsize-32.svg",
    "AppList.targetsize-48.svg",
    "AppList.targetsize-256.svg",
    "ImageLogo.scale-100.svg",
    "ImageLogo.scale-125.svg",
    "ImageLogo.scale-150.svg",
    "ImageLogo.scale-200.svg",
    "ImageLogo.scale-400.svg",
    "ImageLogo.targetsize-16.svg",
    "ImageLogo.targetsize-24.svg",
    "ImageLogo.targetsize-32.svg",
    "ImageLogo.targetsize-48.svg",
    "ImageLogo.targetsize-256.svg"
)


foreach($file in $files)
{
    if (!(Test-Path $file))
    {
        throw "File not found: $file"
    }
    Export-Png $file
}

Copy-TargetSizeLightUnplated "AppList"



