# ByteBrawl physics and limb hurtboxes

Integrate physics rules from LPuehringerStudent/ByteBrawl at 9c84125651c7f72f919f325d433d6187bb5fd77c. User selected: publish the complete Soldier game as a separate `soldier/` Godot project on ByteBrawl branch `feat/soldier-physics-hitboxes`; add thigh and shin damage-receiving hurtboxes.

Share an engine-independent C# physics helper with ByteBrawl's native controller/FSM/rules. Adopt its immediate directional movement, neutral airborne drift and pre-hit-percentage knockback formula. Retain Soldier-scale speed/gravity/jump tuning, mobile attack steering, coyote/buffering, double jump, three-hit combo, damage 2/3/4 and all animation timings. The world collision capsule stays 12 radius / 108 height. Attack circles stay radius 9 on the same fists/foot and active frames.

Replace the Soldier's coarse damage rectangle with fifteen per-part capsules (everything except backpack), including both thighs and shins. Derive pose-specific geometry from existing pixels and joint metadata for all 57 frames. Mirror/follow animation without rotating or changing artwork. Deduplicate damage by fighter, not limb. Preserve dummy behavior. Add F3 debug rendering for hurtboxes and active attack circles.

Workflow: documentation commit; shared physics milestone; hurtbox metadata and gameplay integration milestone; tests/visual validation; publish branch with complete playable Soldier project and verification report. No sprite changes, so existing Pixelloid-approved assets are copied byte-for-byte; no new raster processing required. Preserve unrelated local files and run timing edit. ByteBrawl's original game remains playable and its unit/smoke suites must pass.

## Verification (2026-09-25)

- Shared ByteBrawl physics: **67 unit tests pass**; original arena and limb-rig smoke scenes both print `SMOKE PASS`.
- Published-layout Soldier copy: **435 movement checks**, **120 training/combat checks**, **599 limb/physics checks**, and **72 rendered resize checks**; zero failures. Local limb run includes one additional successful debug-image capture (600 checks).
- Limb tests cover every clip/frame in both facings, valid capsules, actual thigh/shin point queries, own-body exclusion and a single damage event despite multiple simultaneous limb contacts.
- All **23 PNG assets are byte-identical** to Soldier baseline d6bbbe3 in both repositories. All 49 pre-existing asset files preserve data/import settings (text comparisons normalize line endings). 57 pose records and existing atlas pixels preserved. See `BYTEBRAWL_VERIFICATION.json`.
- C# builds succeed for both projects. The new ByteBrawl source is shared through a compile link in the child project's csproj; its root project excludes child C# files. Game runs independently via `godot --path soldier`.
- Behavior changes: ground starts/stops immediately; neutral airborne movement retains horizontal speed; damage-scaled knockback uses damage before the hit. Soldier-scale gravity/jump tuning, attack steering, 2/3/4 damage and original visuals remain.
- F3 debug image inspected: capsules fit the character's parts, with both upper/lower legs included. Debug graphics are off by default.
