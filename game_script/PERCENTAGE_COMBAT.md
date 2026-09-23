# Percentage combat and off-stage recovery

## Approved behavior

Replace finite HP with accumulating percentage for Soldier and dummy, starting at 0% and continuing past 100%. Jab adds 5%, cross 7%, preserving once-per-target impact hits and current 200/300 ms animations. No healing timer and no death at 100%.

Reference: Brawlhalla official Patch 9.10 (July 30, 2025) reduced Unarmed Neutral Light total damage from 14 to 12: https://www.brawlhalla.com/news/wu-shangs-ascension . Our 5+7 split adapts that total to two hits; it is not a claim that each Brawlhalla punch deals these amounts.

Launch speed in pixels/second, using post-hit damage percentage: jab = 150 + 3*percent at 25 degrees upward; cross = 230 + 4.5*percent at 35 degrees upward. Hitstun lasts 150/200 ms. These are our stage-specific tuning values, not Brawlhalla's exact force formula. Use a shared receiver for both combatants; exclude self-hits. Dummy knockback toggle remains independent and keeps its saved value.

Remove HP bar, use damage counters with white/yellow/orange/red progression and +5%/+7% floating feedback. Show Soldier damage in fixed HUD. Position-only dummy return after two seconds preserves percentage. Manual reset and knockout reset position, damage and motion; training respawns remain unlimited.

Shared knockout boundaries: x=-240..1200, y=-300..850. Remove the screen-edge clamp. Soldier-only camera: dead zone x=280..680,y=150..390; center bounded x=240..720,y=90..450; exponential smoothing rate 6/s, integer render position, zoom 1x. Snap to original framing on Soldier reset/respawn. HUD remains fixed. Show a wooden-head edge icon, direction arrow and percentage when the dummy is entirely off-screen.

Airborne punch startup adds forward horizontal momentum: jab 70 px/s, cross 100 px/s, capped at 360 px/s. Same-direction attack steering preserves boosted speed; opposite steering and neutral braking stay available. No upward boost or gravity changes. Grounded attack motion and finite combo rules stay intact.

## Workflow

Commit documentation, then shared percentage combat, then camera/recovery milestones. Reuse existing pixel assets and Pixelloid exports without modifying artwork. Adapt tests for both receivers, percentage growth, launch scaling and hitstun; all boundaries and resets; settings; air momentum and unchanged jump physics; camera limits, icon and pause. Run movement/combat suites, inspect low/high damage gameplay, record verification, commit, push and launch. Preserve unrelated local edits. Baseline: 13e883a.

## Verification result

Implemented shared `IDamageReceiver` / `DamageState` and named jab/cross profiles. The Soldier now has a hurtbox, percentage display and hitstun; the dummy remains passive. Positive percentage hit numbers, color progression and last-hit information replace HP. Knockout checks take precedence over the dummy's automatic position return, and reset clears hitstun, motion and fractional sprite offsets. Saved training toggles remain compatible.

Godot training/percentage/camera suite: **118 checks, zero failures**, including five saved visual captures. Existing movement/animation suite: **262 checks, zero failures**. Total: **380 checks**. C# build: zero warnings/errors. Tests cover post-hit launch magnitude at 0/50/100/200%, both facings, exact hitstun duration, self-hit exclusion and actual hits on another Soldier, each knockout edge, reset precedence, horizontal recovery boosts/cap with unchanged vertical motion, bounded 1x camera, offscreen indicator directions, partial visibility and pause behavior.

Visually reviewed low/high percentage readouts, the wooden-head indicator, and camera tracking outside the stage. All files under `art/` and `game/assets/` are unchanged against `13e883a`, as is `FistCombo.cs`; the local uncommitted walk-cadence adjustment is preserved. No new bitmap artwork was needed, so existing verified Pixelloid exports were reused.

Reproduce with the C# build, then Godot `res://tests/movement_smoke.tscn` and `res://tests/training_smoke.tscn`. Use a graphical renderer and `-- --capture-training` for the five visual checks. Captures are stored under ignored `artifacts/`.
