# Stage 1 — scrapyard platform

## Requirements

- Use `reference_pics/stage_1.jpg` as the main platform.
- Remove its baked checkerboard with deterministic pixel processing. Preserve original foreground RGB and native resolution; no generated artwork, palette reduction, blur or fractional sprite scaling.
- Align collision with the visible front deck lip and its endpoints. Keep the props and hanging machinery decorative.
- Preserve all Soldier artwork, animations, combat controls and unrelated local changes.
- Follow Code as Pixel Art validation, Pixelloid before Godot import, milestone commits, testing, push and launch.

## Workflow

1. Inspect the JPEG, document requirements and commit the reference.
2. Decode the JPEG once, remove border-connected checkerboard using a color mask, inspect transparency at native resolution, and store an editable pixel source. Record crop and collision coordinates in one stage layout file.
3. Validate/render the full-resolution source with Code as Pixel Art. The character's 12-color constraint does not apply to this supplied detailed stage: preserve the JPEG's decoded colors.
4. Run the real Pixelloid engine at pixel size 1. Verify exact RGBA preservation before importing its PNG into Godot. Commit the art and Pixelloid milestones separately.
5. Replace the line with the stage sprite and matching collision polygon. Use integer placement, nearest filtering, lossless import and no mipmaps. Derive spawn and edge tests from the same layout.
6. Test both edges, slopes, falling, respawn, jumping and all existing animation/attack behavior. Inspect the rendered scene, record results, commit, push and launch.

## Limits

The reference is a JPEG, so its existing compression artifacts cannot be recovered. The workflow preserves retained decoded pixels exactly; it does not claim to restore missing detail.
