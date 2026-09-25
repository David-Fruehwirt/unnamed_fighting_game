# ByteBrawl physics and limb hurtboxes

Integrate physics rules from LPuehringerStudent/ByteBrawl at 9c84125651c7f72f919f325d433d6187bb5fd77c. User selected: publish the complete Soldier game as a separate `soldier/` Godot project on ByteBrawl branch `feat/soldier-physics-hitboxes`; add thigh and shin damage-receiving hurtboxes.

Share an engine-independent C# physics helper with ByteBrawl's native controller/FSM/rules. Adopt its immediate directional movement, neutral airborne drift and pre-hit-percentage knockback formula. Retain Soldier-scale speed/gravity/jump tuning, mobile attack steering, coyote/buffering, double jump, three-hit combo, damage 2/3/4 and all animation timings. The world collision capsule stays 12 radius / 108 height. Attack circles stay radius 9 on the same fists/foot and active frames.

Replace the Soldier's coarse damage rectangle with fifteen per-part capsules (everything except backpack), including both thighs and shins. Derive pose-specific geometry from existing pixels and joint metadata for all 57 frames. Mirror/follow animation without rotating or changing artwork. Deduplicate damage by fighter, not limb. Preserve dummy behavior. Add F3 debug rendering for hurtboxes and active attack circles.

Workflow: documentation commit; shared physics milestone; hurtbox metadata and gameplay integration milestone; tests/visual validation; publish branch with complete playable Soldier project and verification report. No sprite changes, so existing Pixelloid-approved assets are copied byte-for-byte; no new raster processing required. Preserve unrelated local files and run timing edit. ByteBrawl's original game remains playable and its unit/smoke suites must pass.
