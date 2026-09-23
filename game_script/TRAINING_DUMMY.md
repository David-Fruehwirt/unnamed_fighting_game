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

Confirmed by the user: automatic return after two seconds without a hit plus a manual Reset Dummy button.

## Completed workflow and verification

Authored and committed the stand, target-painted torso and carved head separately in `art/dummy.pixel.json`. Code as Pixel Art MCP inspected the source, applied hash-guarded semantic pixel operations in batches of at most 1000, validated it and rendered the previews/export. Pixelloid processed the 80x120 sprite at pitch 1 with identical RGBA output before Godot import. The sprite uses eleven opaque colors and binary transparency.

The dummy sits at (620,334). Hit circles follow the animated far hand for jab impact and near hand for cross impact. Jab does 10 damage; cross does 15. Each punch can hit a target once. Health refills one second after depletion. Independent settings persist in `user://training.cfg`; Escape or Settings opens the pause panel. Auto-reset changes position only, preserving HP; Reset Dummy restores both. Disabling knockback stops horizontal impulse motion; gravity still settles a dummy already in the air. Off-stage rescue always restores the target.

Build: zero warnings/errors. Training suite: **36 checks, zero failures**, including two saved visual checks. Existing movement/animation suite: **262 checks, zero failures**. Reviewed `artifacts/training-hit.png` and `artifacts/training-settings.png`. All Soldier source/imported artwork, attack timing, stage artwork and collision layout are preserved against `86e7f76`; the pre-existing local run-speed edit remains uncommitted.

Reproduce art: run PixelAuthor `dummy stand`, `dummy body`, `dummy head` to prepare operation files in `art/work/`; inspect the source and apply each operation file with Code as Pixel Art and its current expected hash. Validate, render to `art/exports/training/dummy.png`, run PixelAuthor `pixelloid`, then import `art/pixelloid/training/dummy.png` with lossless compression, no mipmaps and alpha-border fixing disabled. No standard image generation is used.

Reproduce tests: build `game/Unnamed Fighting Game.csproj`; run Godot on `res://tests/training_smoke.tscn` and `res://tests/movement_smoke.tscn`. Pass `-- --capture-training` with a graphical renderer to save hit/settings review images. Training tests use a separate temporary settings file.
