# Powered second jump

Approved requirements: one fresh midair jump press; replenish only on landing or respawn. Preserve the first jump, coyote time, landing buffer, combat, character design, palette and body-part sizes. Second jump targets 15% more height and reach: impulse 499, directional speed 253, release clamp 182; first jump remains 465/235/170.

Append eight discrete poses (37–44) using unchanged draw definitions and fixed idle bone lengths. Give the powered jump a tighter tuck and raised guard. Four-frame blue/cyan flames attach beneath both boots and both backpack outlets, only during second-jump ascent. Stop at apex, ceiling, hit, landing or reset; track motion independently of punches. No third jump before ground contact.

Workflow: document and commit; inspect structured source; author integer-grid poses with hash-guarded Code as Pixel Art operations; validate/export/review and commit animation; author separate effects and attachment metadata; process actual Pixelloid at 1:1 before game import and commit; integrate C# and test; verify all 592 previous part frames unchanged; commit, push and launch. No image generation. No body-part redesign, so existing per-part sources remain unchanged.

Baseline: 1b43084783baa66e8e86246be9f95e1dda1c2600. Preserve unrelated local edits, including the local run timing override.

## Verification — 2026-09-24

- Godot C# build and PixelAuthor build: zero warnings/errors.
- Movement suite: **371 checks, zero failures**, including a rendered powered-jump capture.
- Training/combat suite: **113 checks, zero failures**.
- Measured full-held arcs: first jump 84 px high / 168.28 px reach; powered jump 96.41 px high / 193.83 px reach. Improvements: **14.77% height, 15.18% reach**.
- Eight powered body frames and all four exhaust frames observed. Fresh press required; third jump blocked; landing/reset refresh; hits and ceiling contact do not refresh. Landing buffering, ledge jumps, short hops, combat interruption and mirrored outlets verified.
- All **592 prior part frames**, old poses/clips, original part definitions, palette and stage preserved. Largest fixed-bone rounding deviation in new poses: **0.468 px**.
- Actual Pixelloid processed 48 exports at 1:1 with identical RGBA pixels. Godot uses nearest sampling and lossless imports; new effects disable alpha-border rewriting.
- Rendered gameplay image inspected at native scale and enlarged nearest-neighbor detail: two boot and two backpack exhausts connect to the armor; no new armor or size changes.
- Source: `art/soldier.pixel.json`, `art/boot-thrusters.pixel.json`, `art/pack-thrusters.pixel.json`; outlets: `art/thruster-sockets.json`. Preview: `art/previews/double-jump.gif`; game atlases: `game/assets/soldier_frames/` and `game/assets/soldier_effects/`.
- Reproduction: `dotnet run --project tools/PixelAuthor -- double-jump`, then `thrusters`, then `pixelloid`. Validate before copying verified exports into Godot. `node tools/verify-double-jump.cjs` verifies the protected baseline.
