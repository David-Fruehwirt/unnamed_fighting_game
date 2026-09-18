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
