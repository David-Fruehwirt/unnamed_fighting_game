# Mobile attacks and coarse Stage 1 — 22 September 2026

Implement [the requested plan](../next_plan.md): one character preview in a concise README; 150×75 Stage 1, 32 opaque colors and exact 4× display pixels; mobile ground/air jabs with one queued follow-up. Character source pixels, part sizes, effects and frame timings remain unchanged.

## Movement reference

Brawlhalla's official notes describe [momentum carried into attacks](https://www.brawlhalla.com/news/new-background-options-balance-and-magyar-skin-patch-6-10) and [forward acceleration during aerial attacks](https://www.brawlhalla.com/news/celebrate-luck-o-the-brawl-2023-patch-7-05). These motivate preserving momentum; the requested 50% ground speed, 600 px/s² steering, 900 ground braking and 120 air braking are this prototype's specified tuning, not claimed Brawlhalla constants.

## Workflow and preservation

1. Commit the requirements and research.
2. Run Pixelloid medoid sampling at pitch 6 on the padded native Stage 1 master; deterministically median-cut to 32 opaque colors. Save a new editable coarse source, validate, export at 4× and inspect; commit.
3. Verify exports through Pixelloid before Godot import; commit the gate.
4. Implement controller-only attack movement and a one-slot input queue. Keep the full 300 ms jab in every movement state, lock facing per jab, preserve jump/landing physics and clear the queue on reset.
5. Test movement, air attacks, transitions, queue coalescing, reset and pixel imports. Compare character assets against `df557a2` and preserve the existing local walk-cadence edit. Simplify README, record results, commit, push and launch.

## Results

- Stage 1 uses 150×75 logical pixels and exactly 32 opaque colors, exported to 600×300 with uniform 4×4 blocks. Its position, flat surface and spawn are unchanged. The native master and earlier pixel source are retained.
- Real Pixelloid pitch-6 medoid conversion and deterministic weighted median-cut are recorded in `art/stages/STAGE_1_PIXELIZATION.json`. Code as Pixel Art validates the coarse source; final Pixelloid and Godot PNG hashes match.
- Attacks work during movement, preparation, ascent, descent and landing. They preserve momentum, allow steering, hold facing per jab and use the five requested configurable movement settings. One queued press starts the next full jab; holding J alone does not repeat. Reset and respawn clear both states.
- Character artwork, scene, body dimensions, effect sources and animation timing remain unchanged. `SoldierVisual.cs`, including the existing local cadence change, retains SHA256 `95fc00fd9fd7a17e37f4bf51ba725f9707a07b91736790a204f739479a5ba5bc`.
- C# build passed with zero warnings/errors. Godot passed **208 checks**, including momentum, both braking rates, air steering, simultaneous jump/J, takeoff/landing continuity, queue coalescing, facing, reset and prior gameplay checks. Independent stage verification passed.
- README now contains one character image and concise controls. Previous details are retained in [the prototype guide](PROTOTYPE_GUIDE.md).
