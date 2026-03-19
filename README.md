# Military - RimWorld Mod

![Military banner](About/Preview.png)

![RimWorld](https://img.shields.io/badge/RimWorld-1.6-B22222?style=for-the-badge&logo=steam&logoColor=white)
![Version](https://img.shields.io/badge/Version-0.6-1E90FF?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-2E8B57?style=for-the-badge)
![Languages](https://img.shields.io/badge/Languages-EN%20%7C%20RU%20%7C%20ZH-orange?style=for-the-badge)
![Harmony](https://img.shields.io/badge/Requires-Harmony-9B59B6?style=for-the-badge)

A full military command structure for your RimWorld colony.
Ranks. Squads. Patrols. Bodyguards. Response. Training. Scenario missions.

## Overview

Military turns your colony's fighters into a structured military force with real hierarchy and tactical control. Soldiers earn promotions through combat, gain meaningful rank bonuses, can be organized into squads, and can be assigned to patrol routes, defend zones, bodyguard duty, and automatic threat response.

Version 0.6 deepens those systems with morale, mood, social, and command-presence effects so squads feel more immersive without adding extra UI or feature bloat.

## Core Features

- Rank system from Recruit to Lieutenant
- Rank-based stat bonuses and command auras
- Squad creation and leader-follow behavior
- Patrol routes with hostile interruption and automatic resumption
- Bodyguard duty for VIP protection
- Defend-area assignments
- Automatic response to hostile threats against colonists
- Combat training dummies for melee, ranged, or both
- Dedicated Military tab with rank, squad, weapon, patrol, and action controls
- Phantom Strike Force scenario with 3 scripted missions

## v0.6 Highlights

- Rebalanced rank pride into a smaller, more natural passive mood effect
- Added command-presence thoughts like `Under Command` and `Leaderless Unit`
- Added squad relationship thoughts like `Served Together` and `Respects Command`
- Added loss and duty memories like `Lost Squadmate`, `Lost Squad Leader`, `Failed to Protect VIP`, and `Answered the Call`
- Added safe depth to existing systems:
  - promotion and demotion memories
  - field-promotion flavor on automatic squad leader succession
  - training-based squad cohesion
  - patrol contact memory
  - bodyguard trust social effects
- Kept all new effects event-driven and tied to existing systems

## Rank System

Ranks are earned through real combat progression.

- Recruit: starting rank
- Private: +3% shooting accuracy
- Corporal: +5% move speed
- Sergeant: +3% shooting accuracy aura for nearby allies
- Lieutenant: -5% aim time aura for nearby allies

Promotions can also be managed from the Military tab.

## Patrol, Bodyguard, and Response

Patrols:
- assign up to 4 waypoints
- require at least 2 waypoints
- break when enemies are detected nearby
- resume automatically after combat if the route is still valid

Bodyguards:
- assign a soldier to protect a specific VIP pawn
- maximum 2 bodyguards per VIP
- bodyguards follow and react to nearby threats

Response system:
- military pawns can automatically respond to hostile threats against colonists
- responders are restored cleanly after assignments end

## Combat Training

Training dummies support:
- Train Combat
- Train Melee
- Train Ranged
- Cancel Training

Training completion is tracked and now also supports light squad-cohesion flavor in v0.6.

## Phantom Strike Force Scenario

A custom 3-mission narrative scenario against the Helix Corporation.

Mission 1 - No Safe Ground
- Eliminate the entire Helix advance team
- Fail if any operator is lost
- Reward: 300 wood, 200 steel, 10 components, 10 medicine

Mission 2 - Vanguard's Shadow
- Keep Silas Vane alive for 7 days
- Fail if Vane is killed
- Reward: 5,000 silver

Mission 3 - Iron Verdict
- Eliminate Director Kael Voss
- Fail if your entire force is wiped out
- Reward: 2,000 silver, 500 steel, 30 components, 20 glitterworld medicine

## Translations

- English: full
- Russian: full
- Chinese Simplified: full

v0.6 translation coverage includes the new morale, mood, social, and command-presence thought text.

## Requirements

Required:
- Harmony by pardeike

Optional:
- [RH2] Rimmu-Nation2 - Clothing

## Installation

Manual install:

1. Download the latest release from the GitHub Releases page.
2. Extract the mod folder into your RimWorld `Mods` directory.
3. Enable Harmony before this mod in the RimWorld mod manager.

Common mod paths:

```text
Windows: C:\Users\[Username]\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Mods\
Linux:   ~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Mods/
Mac:     ~/Library/Application Support/RimWorld/Mods/
```

## Compatibility

- RimWorld 1.6
- Harmony required
- Compatible with the optional Rimmu-Nation2 clothing integration
- May conflict with mods that heavily replace pawn tables or military AI behavior

## Changelog

### v0.6

- Added morale, mood, social, and command-presence depth to the military systems
- Added situational and memory thoughts for squads, leadership, losses, bodyguards, patrol contact, and successful response duty
- Added promotion, demotion, and field-promotion flavor
- Added training-based squad cohesion effects
- Fixed response gratitude to award only on real successful completion
- Fixed custom memory creation so new military memory thoughts are granted correctly
- Corrected social thought definitions and tightened live-state validation
- Synced release metadata and documentation for the v0.6 update

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

## Support

If you enjoy the mod, consider leaving feedback on GitHub, Steam Workshop, or Nexus Mods.
