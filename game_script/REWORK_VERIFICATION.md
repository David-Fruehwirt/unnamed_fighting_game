# Rework verification

## Stage 1 — 21 September 2026

Only `stage_1.jpg` was used. Its transparent 888×448 export preserves **272,562 original RGB pixels**, without resampling or cropping the silhouette. Code as Pixel Art validation and Pixelloid processing passed before Godot import. The ten-section deck collision follows the artwork to x=82 and x=902; tests verify both slopes, support immediately inside each ledge, no collision outside, falling and respawn. All Soldier assets remain unchanged. C# build: zero warnings/errors. Godot: **185 checks, zero failures**. [Workflow](STAGE_1_WORKFLOW.md), [verification report](../art/stages/STAGE_1_VERIFICATION.json), [in-game preview](../art/previews/stage-in-game.png).

## Jab pass — 20 September 2026

- Inspected all 88 GIF frames through 19 pixel-distinct images, including the six held breakdown frames. [Frame audit](../art/fight-frame-audit.json) records every decoded frame and duration.
- Added five jab poses and synchronized smear/impact effects with 50/100/50/50/50 ms holds. J plays once, holds facing and foot contact, and returns to movement. Jump cancels the grounded jab; reset clears its state and effect.
- All armor definitions, palette, existing source clips, attachment coordinates and **384 idle/walk/jump part frames** remain identical to `404ac55`. The fixed-length attack endpoints have at most **0.519 pixels** of integer rounding; both ankle positions remain fixed. [Preservation report](../art/FIGHT_VERIFICATION.json).
- Code as Pixel Art validates both sources. Pixelloid processed all **41 PNGs** before import with identical input/output RGBA. All seventeen Godot body/effect textures match the processed pixels.
- C# build: zero warnings/errors. Godot harness: **159 checks, zero failures**, including an immediate re-press at recovery completion and all existing movement/jump checks. Inspected the graphical hit capture at 960×540.
- Sources: [body](../art/soldier.pixel.json), [effects](../art/fight-effects.pixel.json). [Combined preview](../art/previews/attack.gif) and [pose sheet](../art/previews/attack-poses.png).

## Eight-frame jump — 20 September 2026

Replaced the twelve-pose jump with the eight poses from `soldier_jump_2.png`. [Current workflow and results](JUMP_8_REWORK.md) record the pose breakdown, unchanged part dimensions and landing behavior. **130 Godot checks passed**, with a clean C# build and Pixelloid processing before import. All 256 idle/walk part frames remain identical to baseline `f0247b4`; the maximum fixed-length endpoint rounding is 0.600 pixels.

## Jump pass — 18 September 2026

Completed all twelve reference poses with unchanged armor definitions and fixed part dimensions. [Jump workflow and results](JUMP_REWORK.md) record the full sequence and authored timing. C# build passed without warnings/errors. **138 Godot checks passed**, including all twelve frames in a full jump, landing recovery, preparation, short-jump release, coyote time and buffered jumps. [Preservation verification](../art/JUMP_VERIFICATION.json) confirms all 256 idle/walk part frames remain identical; Pixelloid processed all 39 exported PNGs before integration.

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
