# Twelve-frame jump rework

## Scope

Recreate all twelve numbered poses in `reference_pics/soldier_class/soldier_jump.png` with the current sixteen armor parts. Preserve each part's size, design and palette; articulate and reposition the existing geometry. Preserve idle and walk artwork, their timing, and the user's local walk-speed setting of 8 runtime ticks.

## Reference review

Read left to right, top to bottom:

1. Prepare: ready guard, bent knees, feet grounded.
2. Push-off: deeper crouch, torso forward, arms swing back/down.
3. Takeoff: extended trailing leg, other knee lifted, arms spread.
4. Ascent 1: both knees bend, feet trail below the hips.
5. Ascent 2: guard returns, one leg extends farther than the other.
6. Apex: both legs tuck, hands rise toward the chest.
7. Descent 1: lower legs reach downward and arms balance the body.
8. Descent 2: legs prepare to receive contact.
9. Landing: deep crouch with one hand reaching toward the floor.
10. Recovery 1: crouched torso starts to lift.
11. Recovery 2: body rises back to guard.
12. Idle: original ready stance.

The reference is a still sheet with no timing metadata. Author short preparation, velocity-aware flight and contact-triggered landing. The numbered sheet's airborne offsets describe the jump arc; Godot physics supplies that translation so it is not baked into the body frames a second time.

## Workflow

1. Record requirements and reference breakdown; commit.
2. Trace the twelve poses, retarget segment directions to the current idle rig's fixed lengths, and re-rasterize the unchanged armor shapes on the integer grid using C#. No standard image generation, image cutouts or runtime bitmap rotations.
3. Inspect and edit the canonical `.pixel.json` through hash-guarded Code as Pixel Art operations. Validate, render all twelve poses and an animated preview, compare with the reference and commit the art milestone.
4. Process exported PNGs with Pixelloid at pixel size 1; verify RGBA identity before copying into Godot. Commit.
5. Integrate the complete sequence, retain responsive movement, verify idle/walk preservation, proportion bounds and landing/air transitions. Build, run tests, inspect the scene, commit and push.

Exact copying of the reference's apparent squash/stretch would change part dimensions. Preserve the requested dimensions and match its joint angles and poses with integer-pixel rounding instead.

## Completed result

- Twelve poses in `art/soldier.pixel.json`, with complete preview `art/previews/jump_sequence.gif` and reference comparison `art/previews/jump-comparison.png`. Source angles and mapped anchors are recorded in `art/jump-reference.json`.
- Runtime atlas: idle 0–7, walk 8–15, prepare 16–17, flight 18–21, descent 22–23, landing/recovery 24–27. Frames 16 and 27 exactly reuse idle frame 0 pixels.
- Grounded preparation lasts four physics ticks (about 67 ms). Coyote jumps remain immediate. Landing recovery starts on actual floor contact; releasing jump during preparation still produces a short jump. Physics supplies the world-space arc, including variable jump height.
- Part definitions, individual editable part sources and palette are unchanged. Fixed-length joint endpoints differ from the idle lengths by at most **0.590 pixels** from rounding. No part touches the canvas boundary.
- All 256 idle/walk body-part frames, their source timing and socket metadata match baseline `440d39c`. The user's local 8-tick walk-speed edit is retained in the working file and kept separate from the jump commit.
- Pixelloid verified **39 PNGs** before game import. All sixteen imported atlases match processed pixels.
- C# build: no warnings/errors. Godot verification: **138 checks, zero failures**. A full held jump visits every one of the twelve poses, and recovery returns to idle. Short jumps, coyote time and jump buffering pass.
- Inspected all twelve comparison poses and the graphical landing capture at `artifacts/jump-landing-game.png`.
