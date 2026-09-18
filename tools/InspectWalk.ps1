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
$audit=for($i=0;$i -lt $count;$i++) {
 [ordered]@{frame=$i;durationMs=[BitConverter]::ToInt32($delays,$i*4)*10;
   decodedPngSha256=(Get-FileHash -LiteralPath (Join-Path $output "frame-$i.png") -Algorithm SHA256).Hash.ToLowerInvariant()}
}
[ordered]@{reference='soldier_walk.gif';frames=$audit;distinctImages=@($audit | Group-Object decodedPngSha256).Count} |
 ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $root 'art/walk-frame-review.json') -Encoding UTF8
$gif.Dispose();$font.Dispose()

# Eight reference poses next to the rendered armored poses at a common viewing scale.
$atlasPath=Join-Path $root 'art/exports/animations/run.png'
if(Test-Path -LiteralPath (Join-Path $root 'art/walk-reference.json')){
 $atlas=[Drawing.Image]::FromFile($atlasPath)
 $sheet=New-Object Drawing.Bitmap(2048,560)
 $g=[Drawing.Graphics]::FromImage($sheet)
 $g.Clear([Drawing.Color]::FromArgb(30,38,52))
 $g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
 $g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
 $font=New-Object Drawing.Font('Arial',11)
 for($i=0;$i -lt 8;$i++){
  $pic=[Drawing.Image]::FromFile((Join-Path $output "frame-$($i*7).png"))
  $x=($i%4)*512;$y=[int][Math]::Floor($i/4)*280
  $g.DrawString("Pose $i / 140 ms: reference | Soldier",$font,[Drawing.Brushes]::White,$x,($y+2))
  $g.DrawImage($pic,(New-Object Drawing.Rectangle(($x+48),($y+26),150,256)),700,120,230,395,[Drawing.GraphicsUnit]::Pixel)
  $g.DrawImage($atlas,(New-Object Drawing.Rectangle(($x+256),($y+24),256,256)),($i*128),0,128,128,[Drawing.GraphicsUnit]::Pixel)
  $pic.Dispose()
 }
 $sheet.Save((Join-Path $root 'art/previews/walk-comparison.png'),[Drawing.Imaging.ImageFormat]::Png)
 $g.Dispose();$sheet.Dispose();$atlas.Dispose();$font.Dispose()
}
