# Third attack: side kick

Approved: J -> jab -> cross -> kick, with fresh presses; one queued follow-up per attack, 200 ms grace after jab/cross, 150 ms hard cooldown after kick. Kick is 300 ms and +7%, with the cross's knockback (230 + 4.5 * post-hit percentage, 35 degrees), 0.20 s hitstun and airborne forward impulse 100.

Reference: `reference_pics/soldier_class/soldier_fight_3.png`, 1774x887, twelve numbered poses read left-to-right, top row then bottom. Observations: 1 guard; 2 raised knee; 3 full extension with thin blue arc; 4 extended foot with bright crescent; 5 starburst at foot; 6 retraction and two curved trails; 7 tucked knee and fading trail; 8 knee lowering; 9 further lowering with last thin trail; 10 foot near ground; 11 return to stance; 12 settled guard. Recreate each articulation using original body shapes/scales, fixed bone lengths and integer endpoints. Reference raster is guidance, not replacement armor.

Append atlas indices 45-56; preserve all 720 existing part frames, old metadata/clips/effects, original part definitions, palette and stage against 0abfc77. Body/effect ticks at 60 Hz: 1/2/1/2/3/2/1/1/1/1/1/2. The still image has no timing; these durations are authored. Foot hitbox radius 9 during numbered frames 3-5, once per target; effects don't deal extra damage. Keep near support foot planted and use far leg for the forward side kick as in the reference. Extend hand/foot/thruster sockets for every new pose.

Workflow: requirements/reference commit; inspect Code as Pixel Art source; C# integer-grid authoring with hash-guarded source operations; render/review/validate all twelve frames and commit animation; separate effect source/preview commit; actual Pixelloid at 1:1 with RGBA identity and commit before game import; C# integration and regression/visual checks; commit, push and launch. No image generation or raster rotation. No part redesign; retain unrelated work including local run timing. README keeps one character image.

## Verification and result

- Added twelve discrete kick poses and separate synchronized blue crescent, impact burst and recovery trails. Runtime duration is 300 ms; active foot frames are 3-5. Damage is +7% once per target; knockback/hitstun match cross.
- All **720 previous body-part frames**, existing clips, original geometry/palette, old effects and stage preserved against baseline 0abfc77. Largest fixed-bone endpoint rounding difference: 0.982 px. New body pixels do not touch atlas borders; supporting ankle stays fixed in every pose.
- Actual Pixelloid verified **50 PNG exports**, with identical RGBA before and after processing. Godot part/effect imports match those pixels. Nearest sampling, lossless import and disabled alpha-border rewriting retained.
- C# build: zero warnings/errors. Movement/animation: **438 checks**; training/combat: **120 checks**; responsive layout: **72 checks**. All **630 checks passed**.
- Verified grace/cooldown boundaries, buffered third input, spam/hold behavior, both facings, one +7% hit across the three active poses, out-of-range misses, interruption/respawn, ground movement, airborne kick, double-jump thrusters and landing during recovery.
- Inspected the complete twelve-frame sheet and in-game extension, impact and recovery captures. The frame timing is authored because the reference is a still image; original part shapes and lengths take precedence over the reference character's different proportions.

Sources: `art/soldier.pixel.json`, `art/kick-effects.pixel.json`, `art/kick-reference.json`. Preview: `art/previews/kick.gif`. Preservation report: `art/KICK_VERIFICATION.json`.

Reproduce with PixelAuthor `kick`, then `pixelloid`; copy verified part/effect exports and pose/socket metadata into Godot. Run `node tools/verify-kick.cjs`, build the C# game, and run the movement, training and responsive smoke scenes. Keep historical verification commands tied to their original atlas sizes.
