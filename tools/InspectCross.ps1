# Decode every frame, record timing, group identical pixels, and trace the white FX.
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$path = Join-Path $root 'reference_pics/soldier_class/soldier_fight_2.gif'
$out = Join-Path $root 'artifacts/fight2-review'
New-Item -ItemType Directory -Force $out | Out-Null
$gif = [System.Drawing.Image]::FromFile($path)
$dimension = [System.Drawing.Imaging.FrameDimension]::new($gif.FrameDimensionsList[0])
$delays = $gif.GetPropertyItem(0x5100).Value
$frames = @()
$masks = @()
for ($i=0; $i -lt $gif.GetFrameCount($dimension); $i++) {
    $gif.SelectActiveFrame($dimension,$i) | Out-Null
    $target = Join-Path $out "frame-$i.png"
    $gif.Save($target,[System.Drawing.Imaging.ImageFormat]::Png)
    $frames += [ordered]@{frame=$i;durationMs=[BitConverter]::ToInt32($delays,$i*4)*10;hash=(Get-FileHash $target -Algorithm SHA256).Hash}
    if ($i -ge 11 -and $i -le 15) {
        $bitmap=[System.Drawing.Bitmap]::new($gif)
        $rows=@()
        for($y=140;$y -lt 284;$y+=4) {
            $row=''
            for($x=0;$x -lt 144;$x+=4) {
                $c=$bitmap.GetPixel($x+2,$y+2)
                $row += $(if($c.R -gt 220 -and $c.G -gt 220 -and $c.B -gt 220){'#'}else{'.'})
            }
            $rows+=$row
        }
        $masks += [ordered]@{gifFrame=$i;origin=@(0,140);pitch=4;center=@(60,212);rows=$rows}
        $bitmap.Dispose()
    }
}
$gif.Dispose()
$frames | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $out 'frames.json')
$masks | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $root 'art/cross-effect-traces.json')
Write-Output "Decoded $($frames.Count) frames; $(($frames.hash | Sort-Object -Unique).Count) unique images; traced five impact states."
