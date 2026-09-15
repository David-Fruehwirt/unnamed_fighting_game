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
| Main Focus | Mech customization + platform fighting + EM Frenzy system |

---

# 2. Classes

- **6 classes planned**
- Each class has its own identity and playstyle.
- Each class has its own weapon pool.
- Some equipment such as **boots, armor and cores** can be used across classes.
- Defensive skills can differ between classes.
- EM Frenzy is a universal combat system and is **not class-dependent**.

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

# 6. EM Frenzy System

**EM Frenzy replaces the previous Element Module system.** There are no EM
pickups, selected EM loadouts, active elements, EM switching or elemental
weapons.

EM Frenzy is earned through combat:

- Hitting an enemy with an attack builds the EM Frenzy meter.
- Successful attacks made in succession build it faster.
- Maintaining a combo increases the Frenzy gain rate, so longer combos reach
  Frenzy sooner.
- Missing, being interrupted or allowing the combo to end slows or resets the
  combo bonus.
- When the meter is full, the player enters **EM Frenzy** and can use the
  character's stronger Frenzy attacks for its duration.
- The exact meter amount, duration and decay values remain subject to tuning.

---

# 9. Combat Structure

## Normal Attacks

- Basic attacks that do not require EMs.
- Fast.
- Lower damage.
- Used for basic combat and combos.

## Heavy Attacks

- **EM Frenzy-powered attacks.**
- Stronger and more specialized.
- Become available while EM Frenzy is active.

## Fists

- Separate from EM weapons.
- Always available.
- Can be used even when an EM is active.
- For Soldier, fists are the **Normal Attack weapon**.

## Soldier Weapons

- **Pistol** — mid-range.
- **Laser Sword** — close-range.
- **Fists** — always available for normal attacks.

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

EM Frenzy can also unlock special defensive skills.

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

These are **normal attacks**, available before and during EM Frenzy.

---

# 13. Soldier — EM Frenzy Attacks

### EM Frenzy Identity

- Attacks become stronger while Frenzy is active.
- Frenzy attacks reward continued pressure and combo execution.
- The exact attack effects are tuned independently for each weapon.

## Frenzy Pistol

| Input | Attack | Properties |
|---|---|---|
| **Side Heavy** | Frenzy Shot | Empowered mid-range projectile |
| **Up Heavy** | Frenzy Burst | Upward anti-air burst |
| **Down Heavy** | Frenzy Recoil | Downward shot with upward recoil; combo starter |
| **Neutral Heavy** | Frenzy Volley | Multi-shot close-range protection |

## Frenzy Laser Sword

| Input | Attack | Properties |
|---|---|---|
| **Side Heavy** | Frenzy Slash | Empowered extended blade; combo starter |
| **Up Heavy** | Frenzy Rise | Upward anti-air slash with strong launch |
| **Down Heavy** | Frenzy Crash | Downward slash and shockwave; combo starter |
| **Neutral Heavy** | Frenzy Spin | Multi-hit spinning slash; combo extender |

---

# 15. Customization & Progression

| System | Design |
|---|---|
| Classes | 6 planned |
| Class Weapons | Each class has its own weapon pool |
| Shared Equipment | Boots, armor, cores can be used across classes |
| Parts | Change stats and gameplay |
| EM Frenzy | Builds from landed hits and accelerates during combos |
| Progression | Research system inspired by Armored Core 6 |
| Unlocks | Stronger/new parts, Frenzy techniques, cosmetics |

---

# 16. Current Build Structure

## General

**Class + Parts + Frenzy combat style**

## Soldier

**Soldier + Armor + Boots + Core + EM Frenzy**

### During Battle

**Before Frenzy → Fists, Pistol and Laser Sword**

**Landed hits → EM Frenzy meter builds**

**Combo → Frenzy gain accelerates**

**Full meter → Empowered EM Frenzy attacks**

### Core Gameplay Loop

**Normal Attacks → Landed hits → Combo acceleration → EM Frenzy → Empowered attacks → Defensive Skill**
