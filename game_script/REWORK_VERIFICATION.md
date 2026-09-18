# Rework verification

## Walk pass — 18 September 2026

Replaced only the movement clip using the eight-frame demonstration in `soldier_walk.gif`. Full reference inspection, proportion mapping, timing and checks are recorded in [the walk workflow](WALK_REWORK.md). C# build passed without warnings/errors; **116 Godot checks passed**. The independent [walk preservation report](../art/WALK_VERIFICATION.json) covers all 224 protected idle/jump/fall part frames. Original armor designs and the movement controller are unchanged.

## Idle pass — 18 September 2026

- Reviewed the 146-frame stance GIF, including the skeleton construction ending at frame 130 and the eight live poses. Exported a [comparison of all eight poses](../art/previews/idle-comparison.png).
- Eight idle frames follow the reference's 100/100/100/100/50/50/50/50 ms timing. Joint positions are traced and rounded onto the 128×128 canvas; the original armor geometry is fitted to these proportions. This is an armored interpretation, not an identical copy of the dummy's contours.
- Original individual part sources, shape definitions, palette and movement controller are unchanged.
- `dotnet run --project tools/PixelAuthor -- verify-idle` passes against baseline `d9709b9993d400836a2ea9b0818618a01d229047`: all 14 non-idle source frames, timings, attachment poses and standalone exports are unchanged. All 224 non-idle atlas part frames have identical RGBA hashes. See [preservation report](../art/IDLE_VERIFICATION.json).
- Pixelloid processed all 36 PNGs before import, with identical input/output RGBA. The sixteen imported Godot atlases match those outputs.
- C# build: zero warnings, zero errors. Godot movement/art harness: **99 checks, zero failures**, including all idle timing boundaries, loop duration, clip transition, raised hands and existing movement checks.
- Inspected the graphical idle capture at the 960×540 viewport (`artifacts/idle-reworked.png`).

## Original implementation — 15 September 2026

Validated on 2026-09-15 with Godot 4.7.2 .NET and .NET SDK 8.0.401.

- All sixteen parts have separate construction, source, export and preview files. Each was committed individually.
- Code as Pixel Art validated the part documents and assembled animation.
- Eighteen discrete frames cover idle, run, jump and fall with sixteen editable part layers.
- Pixelloid 0.1.3 processed 36 PNGs at pixel size 1. Every source/output RGBA hash matches; transparency is binary. See `art/PIXELLOID_REPORT.json`.
- The Pixelloid UI was also used to load and process the run sheet at the safe 1-pixel setting.
- C# build: zero warnings and zero errors.
- Godot C# movement/art harness: **69 checks, zero failures**.
- All sixteen imported textures match Pixelloid pixel hashes exactly. Godot alpha-border modification is disabled to preserve transparent pixels too.
- Graphical idle, run and jump captures were inspected at the 960-by-540 viewport. Local captures are in `artifacts/rework-*.png`; portable animation previews are in `art/previews/`.
- The old generated atlas and unused resources were removed from production; Git history retains them. Gameplay and verification scripts are now C#.

Attacks, loadout UI, elemental effects and EM Frenzy remain future combat implementation.
