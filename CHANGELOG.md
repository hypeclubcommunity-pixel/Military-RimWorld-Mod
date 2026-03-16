# Changelog

## v0.3.0
### New Features
- Bodyguard system: assign Sergeant or below as bodyguard for any colonist
  - Bodyguard follows VIP within 8 cells when undrafted
  - Responds to humanlike threats within 15 cells of VIP (LOS required)
  - Max 2 bodyguards per VIP enforced
  - Gold shield icon displayed above VIP's head
  - Status column shows VIP / Bodyguard labels
- Defend Area system: Lieutenant assigns a rectangular garrison zone to any ranked colonist
  - Defender stays inside zone, attacks humanlike threats inside zone only (LOS required)
  - Ignores threats outside the zone
  - Status column shows Defending label

### Bug Fixes
- Bodyguard gizmo no longer appears for Lieutenant rank
- Bodyguard no longer conflicts with squad follow — bodyguard duty always takes priority
- Designator_DefendArea no longer crashes when Map is null during edge cases
- Bodyguard VIP targeting now correctly rejects enemies, animals, and self-assignment

### Performance
- VipShieldIcon texture cached at startup via StaticConstructorOnStartup — no per-frame ContentFinder calls
- JobGiver_DefendArea threat scan replaced with O(1) CellRect bounding box check
- VipShield.png uses FilterMode.Point for crisp rendering at all zoom levels

### Reliability
- All gameplay Log.Message calls guarded by Prefs.DevMode — silent in normal play
- MilitaryUtility class correctly attributed with [StaticConstructorOnStartup]

## v0.2.0
- Squad system: create squads with Sergeant or Lieutenant leader
- Max 6 members per squad (1 leader + 5)
- Squad members follow leader when undrafted
- Drafted members mirror leader attack target
- Auto-promote on leader death
- Window_SquadManager UI

## v0.1.0
- Rank system: Recruit, Private, Corporal, Sergeant, Lieutenant
- Kill-gated promotions with eligibility and promotion letters
- Mood buffs per rank
- Stat bonuses via XML StatParts
- Patrol system: Lieutenant assigns waypoints, Corporal and below execute
- Military tab with all rank columns
- Rank gizmos: Promote, Demote, Assign Patrol
