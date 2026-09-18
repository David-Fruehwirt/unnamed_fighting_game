Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$output = Join-Path $root 'artifacts/stance-review'
[void][IO.Directory]::CreateDirectory($output)
$gif = [Drawing.Image]::FromFile((Join-Path $root 'reference_pics/soldier_class/soldier_stance.gif'))
$dimension = New-Object Drawing.Imaging.FrameDimension($gif.FrameDimensionsList[0])
$count = $gif.GetFrameCount($dimension)
$delays = $gif.GetPropertyItem(0x5100).Value
$font = New-Object Drawing.Font('Arial',12)
for ($i=0; $i -lt $count; $i++) {
    [void]$gif.SelectActiveFrame($dimension,$i)
    $gif.Save((Join-Path $output "frame-$i.png"),[Drawing.Imaging.ImageFormat]::Png)
}
for ($page=0; $page -lt [Math]::Ceiling($count/20); $page++) {
    $sheet = New-Object Drawing.Bitmap(1500,1920)
    $g = [Drawing.Graphics]::FromImage($sheet)
    $g.Clear([Drawing.Color]::FromArgb(20,25,35))
    $g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    for ($slot=0; $slot -lt 20; $slot++) {
        $i=$page*20+$slot
        if ($i -ge $count) { break }
        [void]$gif.SelectActiveFrame($dimension,$i)
        $x=($slot%5)*300; $y=[Math]::Floor($slot/5)*480
        $dest=New-Object Drawing.Rectangle($x,($y+24),280,440)
        $src=New-Object Drawing.Rectangle(540,280,280,440)
        $g.DrawImage($gif,$dest,$src,[Drawing.GraphicsUnit]::Pixel)
        $delay=[BitConverter]::ToInt32($delays,$i*4)
        $g.DrawString("$i - $($delay*10) ms",$font,[Drawing.Brushes]::White,$x,$y)
    }
    $sheet.Save((Join-Path $output "page-$page.png"),[Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $sheet.Dispose()
}
"Frames: $count; size: $($gif.Width)x$($gif.Height)"
$gif.Dispose(); $font.Dispose()

# Side-by-side comparisons use explicit nearest-pixel lookup at the same scale
# as the C# rig. No smoothing, and both figures face right.
$tracePath = Join-Path $root 'art/idle-reference.json'
if (Test-Path -LiteralPath $tracePath) {
    $trace = Get-Content -LiteralPath $tracePath -Raw | ConvertFrom-Json
    $atlas = [Drawing.Bitmap]::FromFile((Join-Path $root 'art/exports/animations/idle.png'))
    $sheet = New-Object Drawing.Bitmap(2048,560)
    $g = [Drawing.Graphics]::FromImage($sheet)
    $g.Clear([Drawing.Color]::FromArgb(35,43,58))
    $g.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $g.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
    $font = New-Object Drawing.Font('Arial',11)
    for ($i=0;$i -lt 8;$i++) {
        $source = [Drawing.Bitmap]::FromFile((Join-Path $output "frame-$i.png"))
        $reference = New-Object Drawing.Bitmap(128,128)
        for ($y=0;$y -lt 128;$y++) { for ($x=0;$x -lt 128;$x++) {
            $sx=[int][Math]::Round(680-($x-64)/.30)
            $sy=[int][Math]::Round(695+($y-121)/.30)
            if ($sx -lt 580 -or $sx -gt 780 -or $sy -lt 330) {continue}
            $color=$source.GetPixel($sx,$sy)
            if (($color.R -eq 56 -and $color.G -eq 104) -or ($color.R -eq 73 -and $color.G -eq 132)) {continue}
            $reference.SetPixel($x,$y,$color)
        }}
        $x=($i%4)*512; $y=[int][Math]::Floor($i/4)*280
        $g.DrawString("Frame $i / $($trace.frames[$i].durationMs) ms: reference | Soldier",$font,[Drawing.Brushes]::White,$x,($y+2))
        $g.DrawImage($reference,(New-Object Drawing.Rectangle($x,($y+24),256,256)),0,0,128,128,[Drawing.GraphicsUnit]::Pixel)
        $g.DrawImage($atlas,(New-Object Drawing.Rectangle(($x+256),($y+24),256,256)),($i*128),0,128,128,[Drawing.GraphicsUnit]::Pixel)
        $source.Dispose(); $reference.Dispose()
    }
    $sheet.Save((Join-Path $root 'art/previews/idle-comparison.png'),[Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose();$sheet.Dispose();$atlas.Dispose();$font.Dispose()
}
