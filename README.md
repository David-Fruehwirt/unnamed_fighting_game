# Unnamed Fighting Game

A Godot 4.3+ 2D movement prototype: one white-and-cobalt Soldier and a single line platform.

## Play

Import `game/project.godot` in Godot, then press **F6** on `arena.tscn` or **F5** to run the project.

- **A / D** or **Left / Right**: move.
- **Space / W / Up**: jump. Release early for a shorter jump.
- **R**: reset. Falling below the arena also resets the character.

## Character and animation

The [character design guide](game_script/CHARACTER_DESIGN.md) was written before creating the scene. It records the supplied image references, Armored Core 6 and Gundam influences, palette, proportions and Brawlhalla-style gameplay scale.

Open `game/scenes/soldier.tscn` to edit the character. It has **sixteen separate Sprite2D parts**, each referencing a named AtlasTexture resource under `game/assets/soldier/`. All parts use the generated transparent `parts_atlas.png`; the resources select individual regions without duplicating the image.

The joint hierarchy supports animation and replacement of individual parts:

```text
Soldier (CharacterBody2D)
├── CollisionShape2D
├── Visual                         ← flip the entire rig
│   └── Pelvis
│       ├── FarHip / NearHip
│       │   └── Knee
│       │       └── Ankle
│       └── Torso
│           ├── Head
│           ├── Backpack
│           └── FarShoulder / NearShoulder
│               └── Elbow
│                   └── Wrist
│                       └── WeaponSocket
└── AnimationPlayer
```

Each named body joint has its own Sprite child. AnimationPlayer contains editable **idle, run, jump, fall** and **RESET** clips. Animate the joint nodes; preserve Sprite offsets because these align the artwork to the joints. Add future attacks as new clips and attach weapons at the WeaponSocket markers. The capsule controls movement independently of the artwork.

Artwork was created with the built-in image generation tool. Its exact [prompt](game_script/ART_PROMPT.md) and the two original [reference images](reference_pics/) are included for future consistency. These initial cutout animations can be refined or supplemented with hand-drawn frames later.

## Verification

Run the movement checks with your Godot executable:

```sh
godot --headless --path game --script res://tests/movement_smoke.gd
```

Capture the scene with a graphical Godot process:

```sh
godot --path game -- --capture ../artifacts/arena.png
```

Create `artifacts/` first. Add `--run-pose` to capture a mid-run pose.
