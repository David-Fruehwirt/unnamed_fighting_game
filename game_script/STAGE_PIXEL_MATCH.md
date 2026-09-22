# Stage pixels matched to Soldier — 22 September 2026

The Soldier displays each source pixel at one game pixel. Replace Stage 1's 4× expansion with a 600×300 logical source rendered at 1×, matching that density. Keep its displayed size, position, flat collision, spawn and existing 32-color palette. Preserve every character asset, animation and controller setting.

Workflow: record requirements and commit; regenerate from the native transparent master through actual Pixelloid (900×450 padded input, medoid pitch 1.5), map samples to the existing stage palette without dithering, validate the editable source and inspect; commit artwork. Run the final Pixelloid export gate before game import and commit. Verify matching pixel scale, import hashes, preserved character data and gameplay; inspect, commit, push and launch.
