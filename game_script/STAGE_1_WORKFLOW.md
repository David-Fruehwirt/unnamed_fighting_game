# Stage 1 — scrapyard platform

This records the original import. The [pixel rework](STAGE_1_PIXEL_REWORK.md) supersedes native-resolution display and sloped collision with a smaller 2× pixel export and one flat surface.

## Requirements

- Use only `reference_pics/stage_1.jpg` as the main platform. No other stage references are used or changed.
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

## Completed verification — 21 September 2026

- C# mask removes the outside checkerboard and four traced enclosed gaps. The 888×448 crop preserves 272,562 original foreground pixels without clipping, resampling or RGB changes.
- Code as Pixel Art validates the editable full-resolution source. Actual Pixelloid processing at pitch 1 preserves every RGBA pixel; its PNG was imported only after that gate passed.
- Ten collision sections follow the traced front deck lip; game-space endpoints are x=82 and x=902. Sprite placement and collision derive from the same layout. Raycasts verify support just inside each endpoint and no support just outside. Character tests verify walking off each sloped end, falling and respawn; normal capsule overlap can support the character's center slightly beyond the endpoint.
- Godot uses lossless imports, no mipmaps, unchanged transparent RGB, nearest filtering, native sprite size and integer viewport scaling. Default window size is 960×540.
- C# build passed with zero warnings/errors. Godot harness passed **185 checks**. `verify-stage` passes and confirms all Soldier assets are unchanged from `5f438cc`; `verify-fight` still passes.
- Inspected [the full scene](../art/previews/stage-in-game.png) and magnified ledge alignment. [Source verification](../art/stages/STAGE_1_VERIFICATION.json) records hashes and retained-pixel counts.
