# Soldier cross punch and finite combo

## Requirements and reference audit

Use soldier_fight_2.gif for attack two, preserving all sixteen armor definitions, palette, part scales, and idle bone lengths. Preserve all existing clips and the stage. Reference poses must be retargeted to fixed existing bone lengths; the dummy's contours cannot be copied without changing the Soldier design.

All 117 GIF frames were decoded and grouped by identical pixels: 24 distinct images. Frames 0-7 show idle, 8-15 show the cross, subsequent cycles repeat these, and 109-116 hold the labeled breakdown. The eight attack frames last 50/50/50/50/50/100/50/50 ms (450 ms). Mirror reference directions once. Reproduce load, pull, smear, two hit-effect states, follow-through, recovery and overshoot, including separate smear/impact effects.

## Combo contract

- Fresh J starts attack one (300 ms). Holding J never repeats.
- One second press during attack one is buffered until its end. Additional presses coalesce.
- A fresh press within 200 ms after attack one ends also starts attack two; normal movement resumes during this grace period.
- At 200 ms the grace expires; a later press starts attack one.
- Attack two lasts 450 ms. Discard all presses during attack two and its subsequent 150 ms cooldown. At cooldown expiry accept a fresh press to start attack one.
- Preserve mobile/air attacks and vertical physics. Reset and respawn clear the entire combo.
- This is animation/input sequencing, without damage, hit detection or EM Frenzy changes.

## Workflow and verification

1. Commit this document and the reference.
2. Use C# integer-grid authoring and hash-guarded Code as Pixel Art operations. Append eight poses to the existing 29, preserve first-punch effects, inspect all new frames, commit art/tool milestone.
3. Process exports using actual Pixelloid at 1:1 pixel pitch, validate RGBA equality and transparency, commit before Godot import.
4. Integrate clips, effects, sockets and finite combo in C#. Test boundaries, spam, holding J, reset, movement, air and landing. Verify previous 464 body-part atlas frames and all protected assets against baseline a6d8b2e.
5. Commit integration and verification milestones, push, and launch Godot. Preserve the local run-speed change and other unrelated work. README stays brief with one character preview.

## Result

Implemented an eight-frame, 450 ms cross, its separate smear/ring source and matching hand sockets. Source poses use unchanged armor definitions and bone lengths, with at most 0.692 pixels of integer rounding. All 464 existing body-part frames, original attack effects, idle/walk/jump clips and stage assets are preserved against a6d8b2e.

Pixelloid processed 44 PNG exports at 1:1 pitch with identical RGBA hashes. Godot uses lossless imports, nearest filtering and no alpha-border rewriting. The 37-frame body atlas and both effect textures match verified exports.

Build: zero warnings/errors. Godot: **261 checks, zero failures**. Checks cover exact 200 ms grace/150 ms cooldown boundaries, the 450 ms cross, held J, spam, fresh restart, both facings, steering, air attacks, short jumps, landing, reset, respawn, per-frame timing and source/import preservation. The first test run caught Godot's default alpha-border rewriting on the new effect texture; disabling it restored exact RGBA equality and the full suite then passed.

Reviewed all distinct GIF frames, the authored eight-frame sheet and an in-game impact capture. Preview: `art/previews/cross.gif`. Sources: `art/soldier.pixel.json`, `art/cross-effects.pixel.json`. Preservation report: `art/CROSS_VERIFICATION.json`.

Reproduce: `tools/InspectCross.ps1`, then PixelAuthor `cross`, `pixelloid`, copy verified body/effect exports and poses into Godot, and run `verify-cross`. Build the Godot C# project and run `res://tests/movement_smoke.tscn`. Set `DOTNET_ROLL_FORWARD=Major` when invoking the authoring tool on this workstation.
