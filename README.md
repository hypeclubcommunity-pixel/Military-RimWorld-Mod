<div align="center">

<img src="About/Preview.png" alt="Military banner" width="100%">

# Military

**Build a real chain of command for your RimWorld colony.**

Ranks | Squads | Patrols | Bodyguards | Threat Response | Training | Scenario Operations

<p>
  <img src="https://img.shields.io/badge/RimWorld-1.6-B22222?style=for-the-badge" alt="RimWorld 1.6">
  <img src="https://img.shields.io/badge/Version-0.6-1E90FF?style=for-the-badge" alt="Version 0.6">
  <img src="https://img.shields.io/badge/Requires-Harmony-6A5ACD?style=for-the-badge" alt="Requires Harmony">
  <img src="https://img.shields.io/badge/Languages-EN%20%7C%20RU%20%7C%20ZH-D97706?style=for-the-badge" alt="Languages EN RU ZH">
  <img src="https://img.shields.io/badge/License-MIT-2E8B57?style=for-the-badge" alt="MIT License">
</p>

**[Latest Release](../../releases/latest)** | **[Issues](../../issues)** | **[Feature Requests](../../issues)** | **[v0.6 Highlights](#v06-highlights)**

</div>

---

> Military turns your combat colonists into a structured fighting force with rank progression, squad leadership, tactical assignments, and a stronger morale and identity layer built around the systems already in the mod.

## Why Military Feels Different

Military is built around **discipline, command, and battlefield roles** rather than raw stat inflation.

- Promote soldiers through real combat progression
- Organize colonists into squads with leaders and followers
- Assign patrol routes, defend areas, and VIP protection
- React automatically to hostile threats against colonists
- Add morale, memory, and social depth to military service

## Core Pillars

<table>
<tr>
<td width="33%" valign="top">

### Command

- 5-rank progression from Recruit to Lieutenant
- Rank bonuses and leadership auras
- Promotion and demotion control
- Better command presence in v0.6

</td>
<td width="33%" valign="top">

### Tactical Duty

- Squad management
- Patrol routes with waypoint control
- Bodyguard and defend-area assignments
- Automatic response to nearby threats

</td>
<td width="33%" valign="top">

### Immersion

- Morale and mood effects
- Social thoughts tied to service and command
- Squad loss and protection memories
- Training and field-duty flavor

</td>
</tr>
</table>

## v0.6 Highlights

| New in v0.6 | Why it matters |
| --- | --- |
| Morale and command-presence thoughts | Squads feel more human, reactive, and structured |
| Squad-loss and response-duty memories | Combat outcomes leave lasting impact |
| Promotion, demotion, and field-promotion flavor | Military hierarchy feels more alive |
| Training cohesion effects | Training supports unit identity, not just skill gain |
| Bodyguard trust and protection flavor | Escort duty feels more meaningful |
| Memory and response logic fixes | New thoughts now grant correctly and safely |

## What You Can Do

### Rank and Command

| Rank | Bonus | Type |
| --- | --- | --- |
| Recruit | None | Starting rank |
| Private | +3% shooting accuracy | Personal |
| Corporal | +5% move speed | Personal |
| Sergeant | +3% shooting accuracy | Aura |
| Lieutenant | -5% aim time | Aura |

Promotions are earned through military progression, and senior ranks help shape how squads perform and feel in the field.

### Squad Leadership

- Create squads and assign leaders
- Keep soldiers organized under a clear command structure
- Benefit from command-presence effects in active units
- Feel the consequences when squadmates or leaders are lost

### Patrols, Defense, and Bodyguards

#### Patrols

- Set up routes with up to 4 waypoints
- Minimum 2 waypoints required
- Patrols break for nearby hostiles
- Troops resume patrol after combat when the route is still valid

#### Defend Area

- Mark a zone with two map corners
- Assign soldiers to hold and defend that area

#### Bodyguard Duty

- Assign up to 2 bodyguards per VIP
- Escort and protect key pawns
- Gain extra military flavor from trust and protection effects

### Automatic Threat Response

- Military pawns can respond to hostile threats against colonists
- Response assignments restore cleanly after completion
- Successful protection duty now has proper memory and social payoff

### Combat Training

- Train Combat
- Train Melee
- Train Ranged
- Cancel Training

Training supports daily practice and now adds light squad-cohesion flavor in v0.6.

## Phantom Strike Force Scenario

Three linked military missions are included:

| Mission | Objective | Failure Condition | Reward |
| --- | --- | --- | --- |
| No Safe Ground | Eliminate the Helix advance team | Lose any operator | 300 wood, 200 steel, 10 components, 10 medicine |
| Vanguard's Shadow | Keep Silas Vane alive for 7 days | Vane is killed | 5,000 silver |
| Iron Verdict | Eliminate Director Kael Voss | Entire force wiped out | 2,000 silver, 500 steel, 30 components, 20 glitterworld medicine |

## Translations

| Language | Status |
| --- | --- |
| English | Full |
| Russian | Full |
| Chinese Simplified | Full |

v0.6 includes localization support for the newer morale, mood, social, and command-presence thoughts as well.

## Requirements

**Required**

- Harmony by pardeike

**Optional**

- [RH2] Rimmu-Nation2 - Clothing

## Installation

1. Download the latest release from GitHub or Nexus Mods.
2. Extract the `Military` folder into your RimWorld `Mods` directory.
3. Enable **Harmony** before this mod.
4. Start a new game or load an existing save.

```text
Windows: C:\Users\[Username]\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Mods\
Linux:   ~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Mods/
Mac:     ~/Library/Application Support/RimWorld/Mods/
```

## Compatibility

- RimWorld 1.6
- Harmony required
- Compatible with the optional Rimmu-Nation2 clothing integration
- May conflict with mods that heavily replace military AI, pawn-table flows, or command behaviors

## Changelog

### v0.6

- Added morale, mood, social, and command-presence depth to the military systems
- Added new thoughts for squad cohesion, leadership, losses, bodyguard trust, patrol contact, and successful response duty
- Added promotion, demotion, and field-promotion flavor
- Added training-based squad cohesion effects
- Fixed custom military memory creation so new memory thoughts are granted correctly
- Fixed response gratitude so it only awards on true successful completion
- Corrected social thought setup and tightened live-state and same-map validation
- Updated translations, metadata, documentation, and preview art

<details>
<summary><strong>Older Versions</strong></summary>

### v0.5

- Fixed responder and bodyguard duty dropping and reassignment loops
- Added the response system and damage-tracking source files
- Synced English, Russian, and Chinese localization
- Corrected Mission 2 reward text to 5,000 silver
- Updated release metadata and documentation

### v0.4.1

- Added Chinese Simplified translation
- Added Russian translation

### v0.4.0

- Initial public release
- Rank system
- Patrol system
- Bodyguard and defend-area system
- Military tab
- Phantom Strike Force scenario
- Combat training system

</details>

## Support

If you enjoy the mod, leave feedback on GitHub, Nexus Mods, or wherever you follow the project. That helps shape the next update.
