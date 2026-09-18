# Eight-frame walk rework

## Requirements and workflow

- Replace only the movement (`run`) clip with the right-hand eight-frame sequence in `reference_pics/soldier_class/soldier_walk.gif`.
- Preserve the existing armor designs and palette. Use the current idle's part dimensions and bone lengths, including helmet, torso, hands and boots. Preserve idle, jump and fall artwork and playback timing.
- Inspect all GIF frames, trace the eight distinct poses, retarget joint directions to the idle rig, render integer-grid parts with Code as Pixel Art, validate, preview and commit.
- Process through Pixelloid before importing into Godot. Verify protected clips and proportion consistency, build/test, commit, push and launch.

## Reference inspection

Decoded all 224 frames. Full-image pixel hashes identify twelve distinct images; the differences include the four- and six-frame demonstrations. Visually reviewed every distinct image and the eight-frame figure separately. The animation repeats a 56-frame presentation four times.

The right-hand figure changes at GIF frames **0, 7, 14, 21, 28, 35, 42, 49**. Each GIF frame lasts 20 ms, so each authored pose lasts **140 ms**, for a **1120 ms** loop. The static bottom row also shows all eight poses.

The walk faces right, matching the game's source direction. Trace contact, recoil, passing and lift for each leg, with opposite arm swing. Retarget the reference's angles onto the idle's fixed segment lengths; integer endpoints introduce up to one pixel of length rounding. Keep the idle's armored contours rather than copying the dummy's skin outline.

## Timing

The canonical source clock changes from 60 to 300 ticks/second, scaling existing tick counts by five without changing their real durations. This represents 140 ms exactly (42 ticks) alongside the idle's 100/50 ms holds. Runtime movement uses 140 ms at normal speed; existing speed-dependent playback remains.
