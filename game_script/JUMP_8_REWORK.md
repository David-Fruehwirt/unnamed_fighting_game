# Eight-frame jump — 20 September 2026

## Requirements and reference review

Use `reference_pics/soldier_class/soldier_jump_2.png`. Read all eight poses left to right: prepare, push-off, ascent 1, ascent 2, apex, descent 1, descent 2, land. Preserve the existing armor geometry, palette, part dimensions and fixed limb lengths. Preserve idle/walk art and the local walk-speed edit.

The new reference keeps the leading fist raised and the trailing arm back throughout the jump. Preparation and landing use a moderate crouch; it has no hand-on-floor landing or separate recovery poses. The displayed height comes from the jump arc, which remains supplied by Godot physics rather than being added to each sprite a second time.

## Workflow

1. Inspect the complete reference and each pose; record requirements and commit.
2. Retarget the eight joint configurations onto the existing fixed-size armor parts with C# integer-grid drawing. Preserve source parts and stable layer IDs. Apply hash-guarded Code as Pixel Art operations to the canonical assembly.
3. Validate, render a comparison and eight-frame GIF, inspect all poses, and commit the art milestone.
4. Process exports with Pixelloid at pixel pitch 1. Verify matching RGBA hashes before game import; commit.
5. Integrate preparation, four rising/apex poses, two descending poses and one landing pose. Check all eight during a full jump, short jumps, coyote time, buffering, bounds, fixed part lengths and idle/walk preservation. Commit, push and launch.

The image supplies poses but no timing. Keep the current short grounded anticipation and responsive physics; use a short landing hold before returning to idle. Match the reference's articulation using unchanged armor sizes, allowing integer-pixel rounding rather than copying its apparent part distortion.

## Completed result

- Exactly eight poses in `art/soldier.pixel.json`; complete preview `art/previews/jump_sequence.gif`; comparison `art/previews/jump-comparison.png`; traced directions and anchors in `art/jump-reference.json`.
- Atlas indices: idle 0–7, walk 8–15, prepare 16, push-off/ascent/apex 17–20, descent 21–22, land 23. Grounded anticipation stays about 67 ms; the landing hold is about 133 ms, followed by idle. No separate recovery frames remain.
- Original part source files, geometry definitions, color palette and drawing dimensions remain unchanged. Maximum segment-length difference is 0.600 pixels from integer endpoint rounding; every pose fits within the canvas.
- All 256 idle/walk part-frame pixel comparisons, source clip timing and socket metadata match baseline `f0247b4`. The user's local 8-tick walk cadence remains separate from the jump commit.
- Pixelloid verified 39 PNGs before import. Godot imported pixels match the processed outputs.
- C# build: no warnings/errors. Godot checks: **130 passed, zero failures**, including all eight poses during a full jump, short jumps, coyote time and buffering. Inspected the eight-pose comparison and `artifacts/jump-eight-land.png` in-game capture.
