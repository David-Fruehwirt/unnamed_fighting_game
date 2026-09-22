# Stage 1 pixel rework — 21 September 2026

Historical 2× pass. The [current coarse stage and mobile attack pass](MOBILE_ATTACK_REWORK.md) changes the stage to 4×4 pixels and 32 colors while preserving its displayed size and flat collision.

## Agreed requirements

Use only stage_1.jpg. Make the stage approximately 600 pixels wide with visible 2×2 pixels. Replace the sloped deck collision with one invisible straight surface across its widest section. Keep side-view movement and every character file, asset, animation and setting unchanged, including the local walk cadence.

## Workflow

1. Commit these requirements. This pass supersedes the previous native-resolution display and sloped collision requirements.
2. Preserve the transparent 888×448 source. Pad it to 900×450, run real Pixelloid medoid sampling at pixel size 3, and store the resulting 300×150 editable pixel source. Validate with Code as Pixel Art and export at exactly 2× to 600×300. Inspect and commit the art milestone.
3. Record the non-identity Pixelloid conversion and verify the final export before Godot import; commit the gate.
4. Center the sprite at x=480 and align the deck's widest horizontal section (reference row 294) with y=334. Use two endpoints and a 14-pixel-deep rectangular collider. Keep spawn at (480,334); no visible line or depth movement.
5. Verify uniform pixel blocks, full-width flat support, both ledges, falling, respawn and existing movement/attack behavior. Compare character files against the starting state. Inspect the scene, record results, commit, push and launch.

## Results

- Actual Pixelloid pixel size 3, medoid sampling, produces 300×150 logical pixels; the 600×300 export consists entirely of exact 2×2 blocks. Both editable sources validate.
- Sprite placement (180,186); flat deck endpoints (214,334) and (762,334). The existing stage runtime consumes the two-point layout without character or controller changes.
- Final Pixelloid export and Godot texture hashes match. Native master and all character assets/controller remain unchanged from `440eef7`; the local walk-cadence edit remains unstaged and unchanged.
- Build passed with zero warnings/errors. Godot passed **188 checks**, including eleven flat-floor samples and both fall-off edges. The independent stage verifier passed. Inspected the normal-resolution game capture.
