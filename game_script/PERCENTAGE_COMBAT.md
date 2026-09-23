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
