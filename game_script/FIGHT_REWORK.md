# Soldier jab — 2026-09-20

## Requirements

- Press **J** for the jab in `reference_pics/soldier_class/soldier_fight.gif`.
- Inspect every GIF frame, including the slow pose breakdown. Match the punch, recovery and effects to this reference.
- Preserve all sixteen existing armor designs, palette, part scales and idle bone lengths. Preserve idle, walk and jump artwork and timing, including the local walk-speed edit.
- Use C# integer-grid authoring and Code as Pixel Art operations. Process the exports through actual Pixelloid before Godot import. Commit each medium milestone.

## Reference audit

All 88 decoded frames were inspected by grouping pixel-identical images. Frames 0–12 contain the eight-frame idle followed by five jab poses; 13–51 repeat them. Frames 52–81 repeat the jab with an idle separator. Frames 82–87 hold the six labeled breakdown poses for two seconds each.

The jab uses frames **8–12**: smear **50 ms**, hit **100 ms**, follow-through **50 ms**, recover **50 ms**, overshoot **50 ms**. This matches the first three playback cycles and the printed timing labels. Later playback swaps the hit/follow-through holds; the printed timing is used here. White effects progress through a small impact, expanding ring, broken ring and dotted ring. The initial arm smear remains a separate effect rather than stretching an armor part.

The reference faces left; the source rig faces right. Mirror the traced directions once and fit them to the existing idle bone lengths. Preserve fixed foot contact with two-bone leg fitting. Retargeting to the existing armor and integer grid means the dummy's contours cannot be copied literally without changing the design or dimensions.

## Workflow

1. Commit these requirements and the reference.
2. Trace the five poses and four impact masks. Author the jab and separate effect source, validate, export and inspect the preview. Commit the art/tool milestone.
3. Run Pixelloid at 1:1 pitch and verify identical RGBA output. Commit that gate before copying to Godot.
4. Add a grounded, one-shot J attack, synchronized effects and facing. Keep the movement/jump behavior available; jumping cancels a grounded jab. Update the on-screen controls.
5. Verify protected source frames, part dimensions, all atlas pixels, effects, input, timing, reset and existing movement. Inspect the scene, commit, push and launch.

## Preservation baseline

Commit `404ac55`: twenty-four existing poses, sixteen body layers, all part definitions and the armor palette. The local walk cadence is retained separately from the task's commits.

## Result

Completed all workflow gates: two validated editable sources, five jab poses, separate smear/ring effects, Pixelloid processing of 41 PNGs and J integration. Godot passed 159 checks; the preservation verifier confirms all 384 old body-part frames are unchanged. Maximum bone-length rounding is 0.519 pixels. The attack uses fixed feet and facing; movement resumes after 300 ms and jump cancels immediately. See [verification](REWORK_VERIFICATION.md) and [preview](../art/previews/attack.gif).
