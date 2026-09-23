# Wooden training dummy

## Requirements and defaults

Add a stationary wooden target for testing both J punches. Show a health bar with numeric HP and separate damage numbers for every successful punch. Preserve Soldier artwork, animations, 1.5x attack playback, map and movement. No patrol or idle animation.

Initial tuning: 100 HP; jab 10 damage, cross 15 damage. Only impact frames can hit, using the current fist socket and a small hit circle; each punch can damage each target once. Misses and recovery poses do no damage. Use actual HP removed in the number, clamping HP at zero. Refill HP one second after depletion so testing can continue.

Settings: knockback defaults OFF. Automatic position reset defaults ON, returning the dummy after two seconds without a hit; include a Reset Dummy button that restores position and health immediately. These toggles are independent. No autonomous movement; gravity/impulses apply only when displaced. Falling off the stage always rescues the dummy. Pause gameplay while settings are open. Store toggles locally between launches.

## Asset workflow

1. Commit requirements before authoring.
2. Author a wooden base/post, torso/crossbar and head as named editable layers using C# integer-grid drawing, a dedicated wood palette and Code as Pixel Art hash-guarded operations. Commit each completed part. Source canvas 80x120 at 1:1, with feet at (40,116); no image generation.
3. Validate and inspect the finished sprite. Process through actual Pixelloid at pitch 1, verify identical RGBA pixels, then commit the export gate before Godot import.
4. Add C# target, hit detection, damage feedback and settings. Keep the README brief with one Soldier preview. Commit the integration milestone.
5. Test damage windows, once-per-punch hits, misses, HP/death/refill, settings, knockback and reset behavior, preservation and existing movement. Inspect the scene, record verification, commit, push and launch.

## Position-reset interpretation

Default is automatic return after two seconds without a hit plus a manual Reset Dummy button. A clarification was requested while independent art work continued; amend this section if the user chooses manual-only.
