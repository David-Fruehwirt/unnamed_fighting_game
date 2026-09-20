Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$dir=Join-Path $root 'artifacts/jump-review'
[void][IO.Directory]::CreateDirectory($dir)
$source=[Drawing.Bitmap]::FromFile((Join-Path $root 'reference_pics/soldier_class/soldier_jump_2.png'))
$atlas=[Drawing.Bitmap]::FromFile((Join-Path $root 'art/exports/animations/jump_sequence.png'))
$sheet=New-Object Drawing.Bitmap(2048,1000)
$g=[Drawing.Graphics]::FromImage($sheet)
$g.Clear([Drawing.Color]::FromArgb(20,28,40))
$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$font=New-Object Drawing.Font('Arial',11)
$labels=@('Prepare','Push-off','Ascent 1','Ascent 2','Apex','Descent 1','Descent 2','Land')
$edges=@(0,245,475,685,900,1105,1315,1535,1774)
for($i=0;$i -lt 8;$i++){
 $col=$i%4;$row=[int][Math]::Floor($i/4)
 $left=[int][Math]::Floor($edges[$i]*$source.Width/1774)
 $right=[int][Math]::Floor($edges[$i+1]*$source.Width/1774)
 $crop=New-Object Drawing.Rectangle($left,130,($right-$left),470)
 $frame=$source.Clone($crop,$source.PixelFormat)
 $frame.Save((Join-Path $dir "reference-$i.png"),[Drawing.Imaging.ImageFormat]::Png)
 $x=$col*512;$y=$row*500
 $g.DrawString("$($i+1). $($labels[$i]): reference | authored",$font,[Drawing.Brushes]::White,$x,$y)
 $g.DrawImage($frame,(New-Object Drawing.Rectangle($x,($y+26),$frame.Width,$frame.Height)))
 $g.DrawImage($atlas,(New-Object Drawing.Rectangle(($x+256),($y+228),256,256)),($i*128),0,128,128,[Drawing.GraphicsUnit]::Pixel)
 $frame.Dispose()
}
$sheet.Save((Join-Path $root 'art/previews/jump-comparison.png'),[Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$sheet.Dispose();$source.Dispose();$atlas.Dispose();$font.Dispose()
