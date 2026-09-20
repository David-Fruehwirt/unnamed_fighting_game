# Sprite, animation and C# rework

## Requirements

- Game title: **Unnamed Fighting Game**.
- Convert the Godot gameplay, scene support and tests to **C#**. Use the Godot .NET editor and the installed .NET 8 SDK.
- **Do not use standard image generation.** Author or edit each sprite frame with code, a pixel grid or an algorithmic 1:1 canvas.
- Use **Code as Pixel Art** for structured pixel sources, validation and deterministic exports.
- Use **Pixelloid before importing replacement artwork into Godot**. Record the actual processing method and results; merely installing or opening it is insufficient.
- Rework all sixteen body parts individually: head, torso, pelvis, backpack, near/far upper arms, forearms, hands, thighs, shins and feet.
- Keep editable, named part sources for future equipment and animation work.
- Use the two images in `reference_pics/` for visual guidance. Retain white/cobalt armor, dark joints, a cyan visor, mechanical layering inspired by **Armored Core 6**, angular humanoid styling inspired by **Gundam**, and readable **Brawlhalla-style** gameplay size.
- Target approximately 110–120 visible pixels tall at the 960×540 viewport. Use exact integer scaling and nearest filtering.
- Author idle, run, jump and fall as discrete frames. Do not interpolate or rotate raster sprites. Preserve silhouettes, proportions, palette, anchors and foot placement.
- Keep the simple line platform, horizontal movement, variable-height jumping, landing, facing, reset and automatic respawn.
- Preserve the combat design: one element selected in the loadout, temporary elemental status effects, and EM Frenzy gained from hits more quickly through combos. Combat implementation is outside this visual/movement rework.
- **Commit after each completed body part. Commit after every other medium milestone.** Keep unrelated user work intact.

## Workflow and gates

1. Record these requirements and the updated character guide; commit the documentation before authoring.
2. Verify the pixel tools and Pixelloid workflow. Establish a shared semantic palette, integer canvas, anchors and a C# authoring pipeline; commit the tooling milestone.
3. For each body part: author pixel shapes, inspect the structured source, validate it, render a preview, check silhouette and palette, then commit that part before continuing.
4. Assemble the parts into coherent discrete animation frames. Use stable layer/part IDs and integer timing. Validate and render the complete sheet and animation previews; commit the animation milestone.
5. Process/check the exported artwork through Pixelloid at the intended 1:1 pixel pitch, preserving transparency and avoiding unwanted resampling. Save a reproducible report and export. Only verified exports may enter `game/assets/`. Commit the Pixelloid milestone.
6. Convert the Godot controller, scene support and movement verification to C#. Integrate the verified artwork and keep frame-specific body-part layers/attachment metadata accessible. Commit the integration milestone.
7. Build the C# project, run movement and asset checks, inspect in-game idle/run/jump poses, fix defects, and commit the verification milestone.
8. Push completed commits to GitHub and launch the resulting scene.

## Source conventions

All sixteen parts, Pixelloid processing and C# integration are complete. Each body part was committed separately. The current [eight-frame jump pass](JUMP_8_REWORK.md) leaves twenty-four atlas frames: eight idle, eight walk and eight jump poses. See [verification results](REWORK_VERIFICATION.md) and the root README for editing and run commands.

- `.pixel.json` documents are the editable pixel source of truth.
- Semantic palette entries define armor, shadow, cobalt, joints, visor and highlights.
- Draw definitions/C# tools record the initial construction. Rebuilding from those definitions is explicit so manual pixel-source edits are never silently overwritten.
- PNGs, GIFs and sprite sheets are derived exports.
- Pixel authoring and Pixelloid validation happen outside the Godot asset folder until the validation gate passes.
- The earlier generated atlas and prompt describe the previous prototype; replacement production artwork must not depend on image generation.
