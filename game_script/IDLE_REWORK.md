# Idle stance rework — 18 September 2026

## Scope

### Cleanup follow-up

The latest request supersedes exact reference motion: substantially reduce jitter, remove detached pixels, simplify noisy highlights, and lower the guard toward the waist. Keep the armor identity and all non-idle clips intact. Plant both legs, move the upper body together by at most one integer pixel, and keep each part's raster pattern stable. Merge redundant highlight colors and remove tiny detached islands; retain the visor and armor silhouette. Validate and preview, commit the art milestone, process through Pixelloid, import and verify, then commit and push.

- Preserve the existing white/cobalt Soldier design, palette and sixteen editable part designs.
- Change only idle proportions, articulation and timing to follow `reference_pics/soldier_class/soldier_stance.gif`.
- Use the reference's raised guard, bent knees, head position and individual segment proportions. Mirror its left-facing stance for the game's right-facing source.
- Preserve run, jump and fall frames, timings, poses and movement code exactly.
- Author integer-grid pixels with C# and Code as Pixel Art; process exports through Pixelloid before Godot import. No image generation or runtime bitmap rotation.
- Commit each completed medium milestone. No part redesign is requested in this pass.

## Reference review

`tools/InspectStance.ps1` extracts all 146 frames and numbered contact sheets. Frame numbers are zero-based.

- 0–63: eight-pose live loop repeated eight times. Frames 0–3 last 100 ms each; 4–7 last 50 ms each (600 ms per loop).
- 64–86: presentation transition.
- 87–96: pose breakdown presented in a grid.
- 97–120: presentation transition.
- 121–130: skeleton construction, from head through spine, shoulders, arms, hips, knees and feet. Frame 130 is the complete skeleton.
- 131–137: body filled over that skeleton.
- 138–145: slow pose demonstration, held for 1000 ms per pose. These holds are not the live-loop timing.

## Workflow

1. Inspect every frame and record skeleton landmarks in source-image coordinates. Commit this scope and inspection tool.
2. Record eight discrete poses against those landmarks. Map them to the existing 128×128 canvas with a single uniform skeleton scale and horizontal reflection. Round only at rasterization.
3. Re-pose the existing armor geometry; fit helmet, torso, pelvis, limb lengths and widths to the reference proportions without changing their design definitions. Replace only idle cels through hash-guarded pixel operations.
4. Validate editable source and export an eight-frame idle, comparison preview and synchronized part sheets. Verify preserved non-idle source frames and exports against the starting revision. Commit the animation milestone.
5. Run the actual Pixelloid engine at pixel pitch 1, verify identical RGBA output, then copy verified sheets to Godot. Commit the processing milestone.
6. Integrate idle timing, build C#, run movement and animation checks, inspect the game, commit and push. Keep unrelated reference-file moves intact.

## Accuracy

Reference joints are traced in the original 1024×1024 image. Integer rasterization necessarily rounds their projected positions by at most half a game pixel per axis. Armor remains armor: its contour is not a copy of the reference dummy's skin/clothing silhouette.

## Result

- Completed the eight-pose idle, retained the existing sixteen part designs, and used the reference's 600 ms loop.
- Recorded original-image and mapped landmarks in `art/idle-reference.json`; exported `art/previews/idle-comparison.png` and `art/previews/idle.gif`.
- Passed Code as Pixel Art validation, Pixelloid processing, the independent baseline comparison and all 99 Godot checks. See [verification](REWORK_VERIFICATION.md).
