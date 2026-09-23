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
