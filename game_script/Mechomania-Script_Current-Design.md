# Mech Fighting Game — Full Current Design

> **Important:** The skillset below is specifically for the **Soldier class**. Other classes will have their own weapons, skills and attack sets later.

---

# 1. Core Game

| System | Current Design |
|---|---|
| Genre | 2D pixel-art mech platform fighter |
| Inspiration | Smash Bros.-style platform fighting |
| Initial Mode | 1v1 |
| Stocks | 3 per player |
| Win Condition | Knock opponents off the stage |
| Future Modes | 2v2, Free-for-All |
| Initial Prototype | Soldier |
| Main Focus | Mech customization + platform fighting + EM system |

---

# 2. Classes

- **6 classes planned**
- Each class has its own identity and playstyle.
- Each class has its own weapon pool.
- Some equipment such as **boots, armor and cores** can be used across classes.
- Defensive skills can differ between classes.
- EM-Fusions are **not class-dependent**.

### Current Class — Soldier

- First class being prototyped.
- Mid-range focused.
- **Pistol:** mid-range combat.
- **Laser Sword:** close-range combat.
- **Fists:** always available.

---

# 3. Mech Customization

## Weapons
- Each class has its own weapon types.
- Different weapon configurations can change the build.
- Soldier currently uses:
  - Pistol
  - Laser Sword
- Later, Soldier can have different:
  - Pistol types
  - Sword types
- Weapon types can affect attributes and gameplay differently.

## Boots

### Soldier — Flare Boots
- Small thrusters built into the boots.
- Used for the defensive skill.
- Directional boost:
  - Up
  - Down
  - Left
  - Right

## Armor

### Iron Armor
- Balanced.
- Neither highly mobile nor heavy.
- Roughly 50/50 Defense and Agility.

### Nano Armor
- Extremely agile.
- Almost no defense.

## Core

- Provides attribute buffs and nerfs.
- Can specialize the mech toward certain attributes.
- Example: more Agility at the cost of Defense.

---

# 4. Attributes

| Attribute | Function |
|---|---|
| **Defense** | Resistance to damage and knockback |
| **Agility** | Movement, acceleration and aerial mobility |
| **Power** | Attack strength and knockback |
| **Energy** | How many EM abilities can be used consecutively |
| **Stability** | Stagger resistance and downed recovery speed |

### Stability / Stagger

- Certain weapons and combos can stagger opponents.
- Stagger can cause a **downed state**.
- Higher Stability:
  - Harder to stagger.
  - Faster recovery from being downed.
- Lower Stability:
  - Easier to stagger.
  - Slower recovery.

---

# 5. Progression / Research

Inspired by **Armored Core 6**.

Progression can unlock:

- New mech parts.
- Stronger mech parts.
- New weapon configurations.
- New EMs.
- Cosmetics.

---

# 6. EM — Element Modules

**EM = Element Module**

Players choose **2 EMs** for their build before a match.

### Current / Planned Elements

- Light
- Fire
- Electro
- Dark
- Water
- Time
- Space
- More later

Each EM provides:

- Elemental abilities.
- Passive stat bonuses.
- Different gameplay characteristics.

Some EMs are unlocked later.

Some mech parts only support specific elements.

---

# 7. EM Pickup System

Players **do not start with their EMs active**.

### EM Bubble
- A neutral **gray EM bubble** spawns in the arena.
- Any player can pick it up.
- Picking it up activates one of the player's selected EMs.

### EM Order

**EM 1 → EM 2 → EM 1 → EM 2**

- Starting EM is random.
- Once both are obtained, **R switches between them**.

---

# 8. Losing an EM

- Taking enough damage causes the **currently active EM to disappear**.
- The EM is **not dropped**.
- It simply disappears from the mech.
- The player becomes **elementless**.
- They return to their basic combat state.
- They must pick up another EM bubble to regain an EM.

---

# 9. Combat Structure

## Normal Attacks

- Basic attacks that do not require EMs.
- Fast.
- Lower damage.
- Used for basic combat and combos.

## Heavy Attacks

- **EM-powered attacks.**
- Stronger and more specialized.
- Depend on the active EM and weapon.
- Use EM Energy.

## Fists

- Separate from EM weapons.
- Always available.
- Can be used even when an EM is active.
- For Soldier, fists are the **Normal Attack weapon**.

## EM Weapons

Each active Soldier EM provides:

- **Pistol** — mid-range.
- **Laser Sword** — close-range.

## EM Switching

- **R** switches between EM 1 and EM 2.
- Changes the active elemental versions of Soldier's Pistol and Laser Sword.
- Fists remain available.

---

# 10. Defensive Skill

- Separate defensive ability.
- Has a cooldown.
- Possible functions:
  - Shield
  - Shield dash
  - Defensive movement
  - Other support abilities

### Soldier — Flare Boots

- Uses small thrusters.
- Directional defensive boost:
  - Up
  - Down
  - Left
  - Right

Other classes can have completely different defensive skills.

EM-Fusions can also have special defensive skills.

**Exact defensive system is still undecided.**

---

# 11. SOLDIER — Current Skillset

> **Everything in this section is Soldier-specific.**

## Soldier Identity

**Mid-range fighter**

| Weapon | Role |
|---|---|
| **Fists** | Basic / combo combat |
| **Pistol** | Mid-range |
| **Laser Sword** | Close-range |

---

# 12. Soldier — Normal Attacks

Fists are always available.

| Input | Ground | Air |
|---|---|---|
| **Neutral** | 2 quick punches | Diagonal downward foot kick |
| **Side** | 3-hit forward punch combo | — |
| **Up** | Uppercut / anti-air | — |
| **Down** | Low sweep/kick; combo starter | Downward punch/kick; slight downward movement; can spike |

These are **normal attacks**, not Light EM attacks.

---

# 13. Soldier — Light EM Heavy Attacks

### Light EM Identity

- Faster.
- More precise.
- Lower damage.
- Better combo potential.

## Light Pistol

| Input | Attack | Properties |
|---|---|---|
| **Side Heavy** | Light Shot | 1 fast, thin projectile; low damage; precise mid-range poke |
| **Up Heavy** | Light Burst | 4 small upward shots; low damage each; anti-air |
| **Down Heavy** | Light Recoil | 1 downward shot; slight upward recoil; combo starter |
| **Neutral Heavy** | Light Volley | 8 shots around Soldier; very low damage each; close-range protection |

## Light Laser Sword

| Input | Attack | Properties |
|---|---|---|
| **Side Heavy** | Light Slash | Fast extended light blade; low damage; combo starter |
| **Up Heavy** | Light Rise | Fast upward energy slash; anti-air; slight launch |
| **Down Heavy** | Light Crash | Downward slash + short light shockwave; combo starter |
| **Neutral Heavy** | Light Spin | Fast spinning energy slash; multiple low-damage hits; combo extender |

---

# 14. Soldier — Fire EM Heavy Attacks

### Fire EM Identity

- Slower.
- Higher damage.
- Higher knockback.
- Small fire effects/explosions.
- **Not huge explosions** — large explosives are intended for the future **Bombardier** class.

## Fire Pistol

| Input | Attack | Properties |
|---|---|---|
| **Side Heavy** | Fire Shot | 1 slower fire projectile; high damage; small impact burst |
| **Up Heavy** | Fire Burst | 3 fire projectiles upward; small impact bursts; medium damage each |
| **Down Heavy** | Fire Recoil | 1 downward fire shot; recoil launches Soldier upward; small fire burst; combo starter |
| **Neutral Heavy** | Fire Volley | 8 small fire shots around Soldier; low damage each; small fire bursts |

## Fire Laser Sword

| Input | Attack | Properties |
|---|---|---|
| **Side Heavy** | Fire Slash | Slower flame slash; high damage/knockback; short range |
| **Up Heavy** | Fire Rise | Powerful upward flame slash; strong launch; combo finisher |
| **Down Heavy** | Fire Crash | Sword slam + small fire burst; strong stagger; combo starter |
| **Neutral Heavy** | Fire Spin | Spinning flame sword; multiple hits; final hit has high knockback |

---

# 15. Soldier — EM-Fusion

## General Fusion Rules

- **Once per battle.**
- Requires a compatible EM combination.
- Not class-dependent.
- Creates a unique temporary weapon/ability.
- Fusion combinations can have completely different effects.
- During Fusion charging:
  - Knockback percentage rapidly increases.
  - Attacks additionally increase knockback by **+1% per attack**.
- Fusion can also have its own defensive skill.

---

## Current Fusion — Light + Fire

### Firework Minigun

- Temporary special weapon.
- Fires fireworks.
- High area damage.
- **50 bullets**.
- Each shot adds **+1% knockback**.
- Ends after all 50 bullets are used.

### Fusion Concept

**Light = speed/precision**

**Fire = damage/area pressure**

**Light + Fire = sustained explosive area control**

---

# 16. Future Fusion Example — Time + Space

### Time + Space → Antimatter

- Creates **Antimatter Gloves**.
- Allows special fist shots.
- Time-limited.
- Not dependent on class.
- Each fist shot:
  - **+5% damage**
  - **+1% knockback**
- Gloves disappear when the Fusion ends.

---

# 17. Customization & Progression

| System | Design |
|---|---|
| Classes | 6 planned |
| Class Weapons | Each class has its own weapon pool |
| Shared Equipment | Boots, armor, cores can be used across classes |
| Parts | Change stats and gameplay |
| EM Compatibility | Some parts support specific EMs |
| Progression | Research system inspired by Armored Core 6 |
| Unlocks | Stronger/new parts, EMs, cosmetics |

---

# 18. Current Build Structure

## General

**Class + Parts + 2 EMs + Fusion**

## Soldier

**Soldier + Armor + Boots + Core + Light EM + Fire EM**

### During Battle

**No EM → Fists**

**Light EM → Light Pistol + Light Laser Sword + Fists**

**Fire EM → Fire Pistol + Fire Laser Sword + Fists**

**Light + Fire → Firework Minigun Fusion**

### Core Gameplay Loop

**Normal Attacks → EM Heavy Attacks → EM switching → Defensive Skill → EM loss/recovery → Fusion**