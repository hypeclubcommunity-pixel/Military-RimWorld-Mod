using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Military
{
    public class GameComponent_MilitaryManager : GameComponent
    {
        public List<SquadData> squads = new List<SquadData>();

        private int missionStartTick = -1;
        private bool m1_event1_done = false;
        private bool m1_event2_done = false;
        private bool m1_raid_spawned = false;
        private bool m1_complete = false;

        // Training session tracking: key = "pawnId_dayOfYear", value = session count
        private Dictionary<string, int> trainingSessions = new Dictionary<string, int>();

        private int m2StartTick = -1;
        private bool m2_quest_scheduled = false;
        private bool m2_raid_done = false;
        private bool m2_complete = false;

        // Mission 3 state
        private int m3StartTick = -1;
        private bool m3_quest_scheduled = false;
        private bool m3_trader_sent = false;
        private bool m3_wave1_done = false;
        private bool m3_wave2_done = false;
        private bool m3_wave3_done = false;
        private bool m3_complete = false;
        private int m3_commanderPawnId = -1;

        public static GameComponent_MilitaryManager Instance =>
            Current.Game?.GetComponent<GameComponent_MilitaryManager>();

        public GameComponent_MilitaryManager()
        {
        }

        public GameComponent_MilitaryManager(Game game)
        {
        }

        // Debug helpers
        public void SetMissionStartTick(int tick) { missionStartTick = tick; m1_event1_done = false; m1_event2_done = false; m1_raid_spawned = false; m1_complete = false; }
        public void ClearAllTrainingSessions() { trainingSessions?.Clear(); }

        public SquadData CreateSquad(string name, Pawn leader)
        {
            if (!SquadData.IsValidLeader(leader))
                return null;

            if (GetSquadOf(leader) != null)
                return null;

            MilitaryStatComp leaderComp = MilitaryUtility.GetComp(leader);
            if (leaderComp == null)
                return null;

            SquadData squad = new SquadData
            {
                squadId = Guid.NewGuid().ToString(),
                squadName = string.IsNullOrWhiteSpace(name) ? "Unnamed Squad" : name,
                leaderPawnId = leader.thingIDNumber,
                memberPawnIds = new List<int>()
            };

            squads.Add(squad);

            leaderComp.squadId = squad.squadId;
            leaderComp.isSquadLeader = true;

            if (Prefs.DevMode)
                Log.Message($"[Military] Squad {squad.squadName} created with leader {leader.LabelShort}");
            return squad;
        }

        public void ScheduleMission1(int currentTick)
        {
            missionStartTick = currentTick;
            m1_event1_done = false;
            m1_event2_done = false;
            m1_raid_spawned = false;
            m1_complete = false;
        }

        public bool DisbandSquad(string squadId)
        {
            SquadData squad = GetSquadById(squadId);
            if (squad == null)
                return false;

            Pawn leader = FindPawnByIdGlobal(squad.leaderPawnId);
            if (leader != null)
                ClearSquadData(leader);

            if (squad.memberPawnIds != null)
            {
                for (int i = 0; i < squad.memberPawnIds.Count; i++)
                {
                    Pawn member = FindPawnByIdGlobal(squad.memberPawnIds[i]);
                    if (member != null)
                        ClearSquadData(member);
                }
            }

            squads.Remove(squad);
            if (Prefs.DevMode)
                Log.Message($"[Military] Squad {squad.squadName} disbanded");
            return true;
        }

        public bool AddMember(string squadId, Pawn pawn)
        {
            SquadData squad = GetSquadById(squadId);
            if (squad == null || pawn == null)
                return false;

            if (pawn.Faction != Faction.OfPlayer)
                return false;

            if (squad.memberPawnIds == null)
                squad.memberPawnIds = new List<int>();

            if (squad.memberPawnIds.Count >= SquadData.MaxMembers)
                return false;

            if (squad.memberPawnIds.Contains(pawn.thingIDNumber))
                return false;

            if (GetSquadOf(pawn) != null)
                return false;

            if (squad.leaderPawnId == pawn.thingIDNumber)
                return false;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return false;

            squad.memberPawnIds.Add(pawn.thingIDNumber);
            comp.squadId = squad.squadId;
            comp.isSquadLeader = false;

            if (Prefs.DevMode)
                Log.Message($"[Military] {pawn.LabelShort} added to squad {squad.squadName}");
            return true;
        }

        public bool RemoveMember(string squadId, Pawn pawn)
        {
            SquadData squad = GetSquadById(squadId);
            if (squad == null || pawn == null || squad.memberPawnIds == null)
                return false;

            if (!squad.memberPawnIds.Remove(pawn.thingIDNumber))
                return false;

            ClearSquadData(pawn);
            if (Prefs.DevMode)
                Log.Message($"[Military] {pawn.LabelShort} removed from squad {squad.squadName}");
            return true;
        }

        public SquadData GetSquadOf(Pawn pawn)
        {
            if (pawn == null || squads == null)
                return null;

            int pawnId = pawn.thingIDNumber;
            for (int i = 0; i < squads.Count; i++)
            {
                SquadData squad = squads[i];
                if (squad == null)
                    continue;

                if (squad.leaderPawnId == pawnId)
                    return squad;

                if (squad.memberPawnIds != null && squad.memberPawnIds.Contains(pawnId))
                    return squad;
            }

            return null;
        }

        public SquadData GetSquadById(string squadId)
        {
            if (string.IsNullOrEmpty(squadId) || squads == null)
                return null;

            for (int i = 0; i < squads.Count; i++)
            {
                SquadData squad = squads[i];
                if (squad != null && squad.squadId == squadId)
                    return squad;
            }

            return null;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            TickMission1();
            TickMission2();
            TickMission3();
        }

        private void TickMission1()
        {
            if (missionStartTick < 0 || m1_complete) return;
            int now = Find.TickManager.TicksGame;
            int elapsed = now - missionStartTick;
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return;

            // EVENT 1 — 60 ticks (~1 second): opening story letter
            if (!m1_event1_done && elapsed >= 60)
            {
                m1_event1_done = true;
                Find.LetterStack.ReceiveLetter(
                    "Military_M1_BurnedLabel".Translate(),
                    "Military_M1_BurnedText".Translate(),
                    LetterDefOf.ThreatBig);
            }

            // EVENT 2 — 600 ticks (~10 seconds): spawn Helix eraser team
            if (!m1_event2_done && elapsed >= 600)
            {
                m1_event2_done = true;
                m1_raid_spawned = true;
                SpawnRaid(map, 300f, HelixCorpFaction, PawnsArrivalModeDefOf.CenterDrop);
            }

            // COMPLETION CHECK — poll every tick after 1200 ticks (~20 seconds).
            // CenterDrop lands instantly; 1200 ticks gives pods time to open and fighting
            // to resolve before we check.
            if (m1_raid_spawned && !m1_complete && elapsed >= 1200)
            {
                if (AllHelixGone(map))
                {
                    m1_complete = true;
                    missionStartTick = -1;

                    // Signal the quest system
                    Find.SignalManager.SendSignal(
                        new Signal("PhantomStrike.Mission01.Success"));

                    // Directly end the M1 quest so it moves to Historical
                    EndQuestByDef("Quest_Mission01_CrashAndRegroup", QuestEndOutcome.Success);

                    // Send success letter + drop prizes
                    Find.LetterStack.ReceiveLetter(
                        "Military_Mission01_SuccessLabel".Translate(),
                        "Military_Mission01_SuccessText".Translate(),
                        LetterDefOf.PositiveEvent);
                    DropM1Rewards(map);

                    ScheduleMission2(Find.TickManager.TicksGame);
                    if (Prefs.DevMode)
                        Log.Message("[Military] M1 complete — prizes dropped, M2 scheduled");
                }
            }
        }

        // Returns true when no living, non-downed Helix Corporation pawns remain as a
        // threat on the map. Pawns executing the ExitMap job are at the map edge leaving
        // — they are no longer fighting and are counted as eliminated for mission purposes.
        // NOTE: Dead pawns are despawned (they become corpses), so we only need to check
        // that no ACTIVE Helix threats remain. The m1_raid_spawned flag already confirms
        // raiders existed before this method is called.
        private bool AllHelixGone(Map map)
        {
            Faction helix = HelixCorpFaction;
            if (helix == null) return false;
            IReadOnlyList<Pawn> spawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < spawned.Count; i++)
            {
                Pawn p = spawned[i];
                if (p == null || p.Faction != helix) continue;
                if (p.Dead || p.Downed) continue;
                if (p.CurJobDef?.defName == "ExitMap") continue;
                return false; // At least one fighting Helix pawn remains
            }
            return true;
        }

        private static void DropM1Rewards(Map map)
        {
            MilitaryUtility.DropStacks(map, ThingDefOf.WoodLog, 300);
            MilitaryUtility.DropStacks(map, ThingDefOf.Steel, 200);
            MilitaryUtility.DropStacks(map, ThingDefOf.ComponentIndustrial, 10);
            MilitaryUtility.DropStacks(map, ThingDefOf.MedicineIndustrial, 10);
        }

        private static void EndQuestByDef(string questScriptDefName, QuestEndOutcome outcome)
        {
            var quests = Find.QuestManager.QuestsListForReading;
            for (int i = 0; i < quests.Count; i++)
            {
                Quest q = quests[i];
                if (q.root?.defName == questScriptDefName && q.State == QuestState.Ongoing)
                {
                    q.End(outcome, sendLetter: false);
                    if (Prefs.DevMode)
                        Log.Message($"[Military] Quest '{q.name}' ended with outcome {outcome}");
                    return;
                }
            }
        }

        public void ScheduleMission2(int currentTick)
        {
            m2StartTick = currentTick;
            m2_quest_scheduled = false;
            m2_raid_done = false;
            m2_complete = false;
        }

        // Used by DevTools to mark M2 as already started (prevents TickMission2 from
        // re-launching another quest after the 168k-tick delay).
        public void ForceM2QuestScheduled()
        {
            m2StartTick = Find.TickManager.TicksGame;
            m2_quest_scheduled = true;
            m2_raid_done = false;
            m2_complete = false;
        }

        // Used by DevTools to cleanly complete M1 without leaving TickMission1 running.
        public void ForceCompleteM1()
        {
            if (m1_complete) return;
            m1_complete = true;
            missionStartTick = -1;
            ScheduleMission2(Find.TickManager.TicksGame);
            if (Prefs.DevMode)
                Log.Message("[Military] ForceCompleteM1: M1 marked done, M2 countdown started");
        }

        private void TickMission2()
        {
            if (m2StartTick < 0 || m2_complete) return;
            int now = Find.TickManager.TicksGame;
            int elapsed = now - m2StartTick;

            // SCHEDULE M2 QUEST — 168,000 ticks (7 in-game days) after M1 ends
            // Silas Vane arrives and the 7-day protection clock begins
            if (!m2_quest_scheduled && elapsed >= 168000)
            {
                m2_quest_scheduled = true;
                Slate slate = new Slate();
                QuestScriptDef questDef = DefDatabase<QuestScriptDef>.GetNamed("Quest_Mission02_VanguardsShadow", false);
                if (questDef != null)
                {
                    Quest quest = QuestGen.Generate(questDef, slate);
                    if (quest != null)
                    {
                        // Add to manager BEFORE firing the initiate signal so the quest
                        // tab shows it the moment the letter appears.
                        quest.SetInitiallyAccepted();
                        Find.QuestManager.Add(quest);
                        Find.SignalManager.SendSignal(new Signal(quest.InitiateSignal));
                        if (Prefs.DevMode)
                            Log.Message("[Military] Mission 2 quest launched — Silas Vane inbound");
                    }
                }
            }

            // MID-RAID — 252,000 ticks from m2StartTick = 3.5 days into the 7-day protection phase
            // (168,000 ticks for the wait + 84,000 ticks = halfway through the 7-day quest timer)
            if (m2_quest_scheduled && !m2_raid_done && elapsed >= 252000)
            {
                m2_raid_done = true;
                Find.LetterStack.ReceiveLetter(
                    "Military_M2_RaidLabel".Translate(),
                    "Military_M2_RaidText".Translate(),
                    LetterDefOf.ThreatBig);
                Map map = Find.AnyPlayerHomeMap;
                if (map != null)
                    SpawnRaid(map, 500f, HelixCorpFaction);
                if (Prefs.DevMode)
                    Log.Message("[Military] M2 mid-protection Helix raid spawned");
            }

            // COMPLETION — 336,000 ticks = quest scheduled (168k) + 7-day timer (168k)
            // Once both events have fired and the quest timer window has passed,
            // mark M2 as complete to stop ticking.
            if (m2_quest_scheduled && m2_raid_done && !m2_complete && elapsed >= 336000)
            {
                m2_complete = true;
                m2StartTick = -1;
                ScheduleMission3(Find.TickManager.TicksGame);
                if (Prefs.DevMode)
                    Log.Message("[Military] M2 tick loop completed — M3 scheduled");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // MISSION 3: IRON VERDICT
        // Timeline (from m3StartTick):
        //   60,000t  (1 day)  → Arms dealer caravan
        //   240,000t (4 days) → Wave 1: scout team (500 pts)
        //   300,000t (5 days) → Wave 2: main force (900 pts)
        //   360,000t (6 days) → Wave 3: final assault + Commander (1400 pts)
        //   After wave 3      → Poll for Commander death = success
        // ════════════════════════════════════════════════════════════════

        public void ScheduleMission3(int currentTick)
        {
            m3StartTick = currentTick;
            m3_quest_scheduled = false;
            m3_trader_sent = false;
            m3_wave1_done = false;
            m3_wave2_done = false;
            m3_wave3_done = false;
            m3_complete = false;
            m3_commanderPawnId = -1;
            if (Prefs.DevMode)
                Log.Message("[Military] Mission 3 scheduled at tick " + currentTick);
        }

        private void TickMission3()
        {
            if (m3StartTick < 0 || m3_complete) return;
            int now = Find.TickManager.TicksGame;
            int elapsed = now - m3StartTick;
            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return;

            // QUEST LAUNCH — 100 ticks after schedule
            if (!m3_quest_scheduled && elapsed >= 100)
            {
                m3_quest_scheduled = true;
                Slate slate = new Slate();
                QuestScriptDef questDef = DefDatabase<QuestScriptDef>.GetNamed("Quest_Mission03_IronVerdict", false);
                if (questDef != null)
                {
                    Quest quest = QuestGen.Generate(questDef, slate);
                    if (quest != null)
                    {
                        quest.SetInitiallyAccepted();
                        Find.QuestManager.Add(quest);
                        Find.SignalManager.SendSignal(new Signal(quest.InitiateSignal));
                        if (Prefs.DevMode)
                            Log.Message("[Military] Mission 3 quest launched — Iron Verdict");
                    }
                }
            }

            // ARMS DEALER — 60,000 ticks (1 day): friendly trader caravan
            if (m3_quest_scheduled && !m3_trader_sent && elapsed >= 60000)
            {
                m3_trader_sent = true;
                Find.LetterStack.ReceiveLetter(
                    "Military_M3_TraderLabel".Translate(),
                    "Military_M3_TraderText".Translate(),
                    LetterDefOf.PositiveEvent);
                SpawnTraderCaravan(map);
                if (Prefs.DevMode)
                    Log.Message("[Military] M3 arms dealer caravan spawned");
            }

            // WAVE 1 — 240,000 ticks (4 days): scout team
            if (!m3_wave1_done && elapsed >= 240000)
            {
                m3_wave1_done = true;
                Find.LetterStack.ReceiveLetter(
                    "Military_M3_Wave1Label".Translate(),
                    "Military_M3_Wave1Text".Translate(),
                    LetterDefOf.ThreatBig);
                SpawnRaid(map, 500f, HelixCorpFaction);
                if (Prefs.DevMode)
                    Log.Message("[Military] M3 Wave 1 spawned (500 pts)");
            }

            // WAVE 2 — 300,000 ticks (5 days): main force
            if (m3_wave1_done && !m3_wave2_done && elapsed >= 300000)
            {
                m3_wave2_done = true;
                Find.LetterStack.ReceiveLetter(
                    "Military_M3_Wave2Label".Translate(),
                    "Military_M3_Wave2Text".Translate(),
                    LetterDefOf.ThreatBig);
                SpawnRaid(map, 900f, HelixCorpFaction);
                if (Prefs.DevMode)
                    Log.Message("[Military] M3 Wave 2 spawned (900 pts)");
            }

            // WAVE 3 — 360,000 ticks (6 days): final assault with Commander
            if (m3_wave2_done && !m3_wave3_done && elapsed >= 360000)
            {
                m3_wave3_done = true;
                Find.LetterStack.ReceiveLetter(
                    "Military_M3_Wave3Label".Translate(),
                    "Military_M3_Wave3Text".Translate(),
                    LetterDefOf.ThreatBig);
                SpawnRaid(map, 1400f, HelixCorpFaction);
                SpawnHelixCommander(map);
                if (Prefs.DevMode)
                    Log.Message("[Military] M3 Wave 3 + Commander spawned (1400 pts)");
            }

            // VICTORY CHECK — after wave 3, poll for Commander death
            if (m3_wave3_done && !m3_complete && m3_commanderPawnId >= 0)
            {
                Pawn commander = MilitaryUtility.FindPawnGlobal(m3_commanderPawnId);
                if (commander == null || commander.Dead || commander.Destroyed)
                {
                    m3_complete = true;
                    m3StartTick = -1;
                    Find.SignalManager.SendSignal(
                        new Signal("PhantomStrike.Mission03.Success"));
                    EndQuestByDef("Quest_Mission03_IronVerdict", QuestEndOutcome.Success);
                    if (Prefs.DevMode)
                        Log.Message("[Military] M3 complete — Commander eliminated!");
                }
            }
        }

        private void SpawnHelixCommander(Map map)
        {
            PawnKindDef cmdKind = DefDatabase<PawnKindDef>.GetNamed("HelixCorp_Commander", false);
            Faction helix = HelixCorpFaction;
            if (cmdKind == null || helix == null)
            {
                if (Prefs.DevMode) Log.Warning("[Military] Could not spawn Helix Commander");
                return;
            }

            Pawn commander = PawnGenerator.GeneratePawn(cmdKind, helix);
            commander.Name = new NameTriple("Director", "Kael", "Voss");

            IntVec3 spot;
            if (!CellFinder.TryFindRandomEdgeCellWith(
                    c => c.Standable(map) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Hostile, out spot))
                spot = CellFinder.RandomEdgeCell(map);

            GenSpawn.Spawn(commander, spot, map);
            m3_commanderPawnId = commander.thingIDNumber;

            // Make commander assault immediately
            LordMaker.MakeNewLord(helix, new LordJob_AssaultColony(helix, true, false, false, false, true), map,
                new List<Pawn> { commander });

            if (Prefs.DevMode)
                Log.Message($"[Military] Helix Commander '{commander.LabelShort}' spawned at {spot}");
        }

        private void SpawnTraderCaravan(Map map)
        {
            TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed("Military_ArmsDealer", false);
            if (traderKind == null)
            {
                if (Prefs.DevMode) Log.Warning("[Military] Arms dealer TraderKindDef not found");
                return;
            }

            // Find a friendly faction to send the trader
            Faction traderFaction = Find.FactionManager.AllFactions
                .Where(f => !f.IsPlayer && !f.HostileTo(Faction.OfPlayer)
                         && !f.defeated && f.def.humanlikeFaction
                         && f.def.caravanTraderKinds?.Count > 0)
                .RandomElementWithFallback(null);

            if (traderFaction == null)
            {
                // Fallback: just drop the trader items via pod
                if (Prefs.DevMode) Log.Warning("[Military] No friendly faction for trader, skipping caravan");
                return;
            }

            IncidentParms parms = new IncidentParms();
            parms.target = map;
            parms.faction = traderFaction;
            parms.traderKind = traderKind;
            parms.forced = true;

            IncidentDefOf.TraderCaravanArrival.Worker.TryExecute(parms);
        }

        public static Faction HelixCorpFaction =>
            Find.FactionManager.AllFactions
                .FirstOrDefault(f => f.def.defName == "HelixCorporation");

        private void SpawnRaid(Map map, float points)
        {
            Faction faction = Find.FactionManager.AllFactions
                .Where(f => f.HostileTo(Faction.OfPlayer)
                        && !f.defeated
                        && f.def.humanlikeFaction)
                .RandomElementWithFallback(null);

            if (faction == null)
            {
                if (Prefs.DevMode)
                    Log.Warning("[Military] SpawnRaid: no hostile faction found");
                return;
            }

            float minPoints = IncidentDefOf.RaidEnemy.minThreatPoints;
            float wealthPoints = StorytellerUtility.DefaultThreatPointsNow(map);
            float finalPoints = Mathf.Max(points, wealthPoints * 0.5f, minPoints);

            IncidentParms parms = new IncidentParms();
            parms.target = map;
            parms.faction = faction;
            parms.points = finalPoints;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;

            bool result = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);

            if (Prefs.DevMode)
                Log.Message($"[Military] SpawnRaid: {finalPoints}pts " +
                    $"faction:{faction.Name} result:{result}");
        }

        private void SpawnRaid(Map map, float points, Faction faction,
            PawnsArrivalModeDef arrivalMode = null)
        {
            if (faction == null)
            {
                SpawnRaid(map, points);
                return;
            }

            float minPoints = IncidentDefOf.RaidEnemy.minThreatPoints;
            float wealthPoints = StorytellerUtility.DefaultThreatPointsNow(map);
            float finalPoints = Mathf.Max(points, wealthPoints * 0.5f, minPoints);

            IncidentParms parms = new IncidentParms();
            parms.target = map;
            parms.faction = faction;
            parms.points = finalPoints;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = arrivalMode ?? PawnsArrivalModeDefOf.EdgeWalkIn;

            bool result = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);

            if (Prefs.DevMode)
                Log.Message($"[Military] SpawnRaid (specific): {finalPoints}pts " +
                    $"faction:{faction.Name} arrival:{parms.raidArrivalMode.defName} result:{result}");
        }

        public void OnPawnRemoved(Pawn pawn)
        {
            if (pawn == null)
                return;

            SquadData squad = GetSquadOf(pawn);

            if (squad != null)
            {
                if (squad.leaderPawnId == pawn.thingIDNumber)
                {
                    squad.AutoPromoteLeader(pawn.MapHeld ?? Find.CurrentMap);
                }
                else if (squad.memberPawnIds != null)
                {
                    squad.memberPawnIds.Remove(pawn.thingIDNumber);
                }
            }

            ClearSquadData(pawn);
            if (squad != null)
            {
                if (Prefs.DevMode)
                    Log.Message($"[Military] {pawn.LabelShort} removed from squad on pawn loss");
            }
        }

        public int GetTrainingSessions(int pawnId, int day)
        {
            string key = pawnId + "_" + day;
            return trainingSessions.TryGetValue(key, out int count) ? count : 0;
        }

        public void IncrementTrainingSessions(int pawnId, int day)
        {
            string key = pawnId + "_" + day;
            if (trainingSessions.ContainsKey(key))
                trainingSessions[key]++;
            else
                trainingSessions[key] = 1;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref squads, "squads", LookMode.Deep);
            Scribe_Values.Look(ref missionStartTick, "missionStartTick", -1);
            Scribe_Values.Look(ref m1_event1_done, "m1_event1_done", false);
            Scribe_Values.Look(ref m1_event2_done, "m1_event2_done", false);
            Scribe_Values.Look(ref m1_raid_spawned, "m1_raid_spawned", false);
            Scribe_Values.Look(ref m1_complete, "m1_complete", false);
            Scribe_Values.Look(ref m2StartTick, "m2StartTick", -1);
            Scribe_Values.Look(ref m2_quest_scheduled, "m2_quest_scheduled", false);
            Scribe_Values.Look(ref m2_raid_done, "m2_raid_done", false);
            Scribe_Values.Look(ref m2_complete, "m2_complete", false);
            Scribe_Values.Look(ref m3StartTick, "m3StartTick", -1);
            Scribe_Values.Look(ref m3_quest_scheduled, "m3_quest_scheduled", false);
            Scribe_Values.Look(ref m3_trader_sent, "m3_trader_sent", false);
            Scribe_Values.Look(ref m3_wave1_done, "m3_wave1_done", false);
            Scribe_Values.Look(ref m3_wave2_done, "m3_wave2_done", false);
            Scribe_Values.Look(ref m3_wave3_done, "m3_wave3_done", false);
            Scribe_Values.Look(ref m3_complete, "m3_complete", false);
            Scribe_Values.Look(ref m3_commanderPawnId, "m3_commanderPawnId", -1);
            Scribe_Collections.Look(ref trainingSessions, "trainingSessions", LookMode.Value, LookMode.Value);
            if (squads == null)
                squads = new List<SquadData>();
            if (trainingSessions == null)
                trainingSessions = new Dictionary<string, int>();

            // Prune stale training session entries (keep only today's)
            if (Scribe.mode == LoadSaveMode.PostLoadInit && trainingSessions.Count > 0)
            {
                int today = GenDate.DayOfYear(Find.TickManager.TicksAbs, 0);
                string todaySuffix = "_" + today;
                List<string> staleKeys = new List<string>();
                foreach (var key in trainingSessions.Keys)
                {
                    if (!key.EndsWith(todaySuffix))
                        staleKeys.Add(key);
                }
                for (int i = 0; i < staleKeys.Count; i++)
                    trainingSessions.Remove(staleKeys[i]);
            }

            // Clear stat aura cache on load to prevent stale cross-save data
            Patches.RankStatPatch.ClearCache();
        }

        private static void ClearSquadData(Pawn pawn)
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            comp.squadId = "";
            comp.isSquadLeader = false;
        }

        private static Pawn FindPawnByIdGlobal(int pawnId)
        {
            return MilitaryUtility.FindPawnGlobal(pawnId);
        }
    }
}
