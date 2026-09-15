# Rework verification

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
