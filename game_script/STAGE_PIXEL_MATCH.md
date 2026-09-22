# Stage pixels matched to Soldier — 22 September 2026

The Soldier displays each source pixel at one game pixel. Replace Stage 1's 4× expansion with a 600×300 logical source rendered at 1×, matching that density. Keep its displayed size, position, flat collision, spawn and existing 32-color palette. Preserve every character asset, animation and controller setting.

Workflow: record requirements and commit; regenerate from the native transparent master through actual Pixelloid (900×450 padded input, medoid pitch 1.5), map samples to the existing stage palette without dithering, validate the editable source and inspect; commit artwork. Run the final Pixelloid export gate before game import and commit. Verify matching pixel scale, import hashes, preserved character data and gameplay; inspect, commit, push and launch.

## Result

- The editable `art/stages/stage_1.matched.pixel.json` is 600×300, exported at 1×. The stage and Soldier sprites both display one source pixel per game pixel.
- Existing 32 colors are reused from the coarse source. Stage size, placement, flat collision and spawn are unchanged; no character artwork, controller or animation changes were made.
- Code as Pixel Art validation and actual Pixelloid processing passed. The independent verifier confirms matching exported/processed/game pixels and preserved character artwork. C# build: zero warnings/errors. Godot: **208 checks, zero failures**, including matching character/stage pixel scale and all existing movement and attack checks.
- Inspected the full game capture at normal resolution. Earlier master and coarse sources remain available.
