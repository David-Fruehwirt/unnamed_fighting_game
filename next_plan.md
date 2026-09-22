# README cleanup, stronger stage pixels, and mobile attacks

  ## Summary

  - Show the character once in a short GitHub README.
  - Rework only Stage 1 into 4×4 pixels with a 32-color palette, retaining its displayed size.
  - Allow the existing jab while standing, moving, jumping and falling, with momentum and one queued follow-up attack.
  - Preserve all character artwork, body-part sizes, animation frames, effects and animation timings.

  Brawlhalla’s official notes describe momentum carried into attacks and aerial attack acceleration. The movement
  below is a defined adaptation for this prototype.

  ## Changes

  ### README and stage

  - Reduce the README to the project title, one-sentence description, controls and one idle GIF. Keep detailed
    documentation in the existing documentation files.

  - Start from the preserved transparent Stage 1 master, padded to 900×450.
  - Run actual Pixelloid medoid sampling at pixel size 6, producing 150×75 pixels.
  - Apply deterministic median-cut quantization to 32 opaque colors, excluding transparency. Use no dithering or
    smoothing.

  - Store the editable pixel source, validate through Code as Pixel Art, and export at exact 4× nearest-neighbor scale
    to 600×300.

  - Preserve the current stage placement, flat collision surface and spawn. Record pixelization and palette settings
    in the workflow reports.

  ### Attacks and movement

  - Separate attack playback from movement state so jumping, falling and landing neither reject nor cancel a jab.
  - Reuse the unchanged five-frame, 300 ms jab in every movement state. Gravity, jump preparation, jump release and
    collision continue normally underneath it.

  - Preserve horizontal velocity when an attack begins. Add these configurable controller settings:

     Setting                                                                   Value
    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
     Ground attack target speed                         50% of normal movement speed
    ─────────────────────────────────────────────────  ──────────────────────────────
     Steering acceleration during attacks                                  600 px/s²
    ─────────────────────────────────────────────────  ──────────────────────────────
     Ground attack braking without directional input                       900 px/s²
    ─────────────────────────────────────────────────  ──────────────────────────────
     Air attack braking without directional input                          120 px/s²
    ─────────────────────────────────────────────────  ──────────────────────────────
     Air attack target speed                                   Normal movement speed

  - Lock visual facing for each jab while allowing directional input to steer horizontal movement. A queued jab
    chooses its facing when it starts.

  - A fresh J press starts immediately when idle. During an attack, remember one follow-up press; additional presses
    coalesce into that pending attack.

  - Start the queued punch immediately after the current animation finishes. Holding J alone does not repeat attacks.
  - Permit simultaneous jump and attack input. Preserve attacks across takeoff and landing; restore the appropriate
    movement animation afterward.

  - Clear active and queued attacks on reset or respawn. Add no new movement mechanics, damage system or character
    animations.

  ## Verification and workflow

  1. Document the requirements and research; commit.
  2. Generate and inspect the stage preview, validate the editable source, and commit the art milestone.
  3. Complete the Pixelloid export gate before Godot import; commit.
  4. Update controller tests for moving attacks, ascent/descent attacks, simultaneous jump/J, landing during attacks,
     steering, momentum, facing, queued presses and reset.

  5. Verify the 32-color limit, uniform 4×4 blocks, unchanged stage dimensions and collision, and exact imported
     pixels.

  6. Compare character assets and animation definitions against the starting state; preserve the existing local walk-
     cadence edit.

  7. Run the build and regression checks, inspect the game, update the concise README and verification notes, commit,
     push and launch.