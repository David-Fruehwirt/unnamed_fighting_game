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
