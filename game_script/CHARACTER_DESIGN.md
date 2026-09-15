# Unnamed Fighting Game — Soldier character design

This guide is the source of truth for the first playable character. Written before creating the scene and its artwork.

## References and direction

- `reference_pics/mech_character.png`: primary silhouette, white and cobalt armor, dark mechanical joints, cyan visor, angular helmet, long armored legs and broad boots.
- `reference_pics/character_animation_reference.png`: side-facing movement, readable poses and consistent armor across animation.
- **Armored Core 6**: functional layered armor, exposed mechanical connections, compact thrusters and believable weight.
- **Gundam**: readable humanoid proportions, angular faceplate, crest and clean armor separation. Create an original Soldier silhouette from these influences.
- **Brawlhalla**: use a compact platform-fighter presentation and readable silhouette at gameplay distance. The reference's 1.2–1.5× note is an artistic proportion guide, not an exact measurement of that game's sprites. Target approximately 110–120 pixels tall in a 960×540 game viewport.

## Visual invariants

- Side / slight three-quarter view facing right; mirror the whole visual rig to face left.
- White armor with cool lavender shadows, cobalt-blue accents, dark navy joints and a narrow cyan visor.
- Angular helmet crest, broad shoulder armor, narrow mechanical waist, articulated knees, substantial blue-toed boots.
- Crisp pixel-art clusters, hard edges, limited palette, nearest-neighbor texture filtering.
- Keep head size, limb lengths, armor shapes and palette identical across poses.
- Dark joint connectors must overlap the armor so bending does not open holes.
- Preserve a clear front/back limb separation; the far limbs are darker.

## Separate animation parts

Create sixteen independently reusable sprite parts: head, torso, pelvis, backpack; near/far upper arms (including shoulder shells), forearms, hands, thighs, shins and feet.

Each limb is a hierarchy of joint pivots: shoulder → elbow → wrist and hip → knee → ankle. The head and backpack attach to the torso. The torso attaches above the pelvis. Sprites remain separate from the physics collision shape. Add weapon attachment points at the hands for future pistol and sword animation.

Use one transparent 4×4 atlas, one isolated part per cell. No labels or drawn assembly guides. Save the exact generation prompt alongside the art. Each Sprite2D uses its own atlas region, sized and positioned at its joint. This makes the same artwork reusable for later animations and equipment replacement.

## First scene

- One simple horizontal platform line and a dark, uncluttered background.
- One Soldier, movable left and right, with jump, gravity, landing and reset after falling.
- A/D or arrow keys to move; Space/W/Up to jump; R to reset.
- Idle, run, jump and fall poses animate the individual joint pivots.
- This scene establishes movement and the character rig. Element attacks and EM Frenzy remain documented future combat work.
