Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$output=Join-Path $root 'artifacts/walk-review'
[void][IO.Directory]::CreateDirectory($output)
$gif=[Drawing.Image]::FromFile((Join-Path $root 'reference_pics/soldier_class/soldier_walk.gif'))
$dim=New-Object Drawing.Imaging.FrameDimension($gif.FrameDimensionsList[0])
$count=$gif.GetFrameCount($dim)
$delays=$gif.GetPropertyItem(0x5100).Value
$font=New-Object Drawing.Font('Arial',12)
for($page=0;$page -lt [Math]::Ceiling($count/12);$page++){
 $sheet=New-Object Drawing.Bitmap(1536,2144)
 $g=[Drawing.Graphics]::FromImage($sheet)
 $g.Clear([Drawing.Color]::FromArgb(24,30,42))
 $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
 for($slot=0;$slot -lt 12;$slot++){
  $i=$page*12+$slot;if($i -ge $count){break}
  [void]$gif.SelectActiveFrame($dim,$i)
  $gif.Save((Join-Path $output "frame-$i.png"),[Drawing.Imaging.ImageFormat]::Png)
  $x=($slot%3)*512;$y=[int][Math]::Floor($slot/3)*536
  $g.DrawString("$i : $([BitConverter]::ToInt32($delays,$i*4)*10) ms",$font,[Drawing.Brushes]::White,$x,$y)
  $g.DrawImage($gif,(New-Object Drawing.Rectangle($x,($y+24),512,512)))
 }
 $sheet.Save((Join-Path $output "page-$page.png"),[Drawing.Imaging.ImageFormat]::Png)
 $g.Dispose();$sheet.Dispose()
}
"Frames: $count, dimensions: $($gif.Width)x$($gif.Height)"
$gif.Dispose();$font.Dispose()
