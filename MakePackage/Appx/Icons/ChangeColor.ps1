# SVGの色変更

$inputDir = "Sources"
#$outputDir = "Sources-blue"
$fromColor = "Red"
$toColor = "Blue"

$outputDir = "$inputDir-$toColor"
if (!(Test-Path $outputDir))
{
    New-Item -Path . -Name $outputDir -ItemType Directory
}


# error to break
trap { break }

$ErrorActionPreference = "stop"

$colorTable = @(
    [PSCustomObject]@{ Red = "#ff7458"; Blue = "#4682b4" }
    [PSCustomObject]@{ Red = "#dd2f0f"; Blue = "#2c5170" }
    [PSCustomObject]@{ Red = "#ff6347"; Blue = "#7ba7cb" }
    [PSCustomObject]@{ Red = "#ffd8d0"; Blue = "#dce8f2" }
    [PSCustomObject]@{ Red = "#ffd4cc"; Blue = "#c0d5e6" }
    [PSCustomObject]@{ Red = "#ffbfb3"; Blue = "#cadbea" }
)

#foreach($item in $colorTable)
#{
#    $red = $item.Red
#    Write-Host "$red, $($item.Red) -> $($item.Blue)"
#}


function Convert-ColorCode($s)
{
    foreach($item in $colorTable)
    {
        $s = $s -replace $item.$fromColor, $item.$toColor
    }
    return $s
}

#Convert-ColorCode "ここに #ffd8d0 と #ff7458 という色があるじゃろ？"


# color change (red -> blue)
function Convert-SvgColorCode($source, $output)
{
    (Get-Content $source) | ForEach-Object { Convert-ColorCode $_ } | Set-Content $output
}


# ディレクトリ以下の svg をまとめて変換



$files = Get-ChildItem -Name $inputDir\*.svg

foreach($file in $files)
{
    Convert-SvgColorCode $inputDir\$file $outputDir\$file
}



