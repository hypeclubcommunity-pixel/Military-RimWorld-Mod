using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace Military
{
    public static class MilitaryDevTools
    {
        [DebugAction("Military", "Give Rank", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void GiveRank(Pawn pawn)
        {
            if (!pawn.IsColonist || !MilitaryUtility.IsEligible(pawn))
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (string r in MilitaryRanks.All)
            {
                string rank = r;
                options.Add(new FloatMenuOption(MilitaryRanks.TranslatedName(rank), () =>
                {
                    comp.rank = rank;
                }));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        [DebugAction("Military", "Add 1 Kill", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Add1Kill(Pawn pawn)
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp != null)
                comp.missionCount += 1;
        }

        [DebugAction("Military", "Add 5 Kills", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Add5Kills(Pawn pawn)
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp != null)
                comp.missionCount += 5;
        }

        [DebugAction("Military", "Add 20 Kills", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Add20Kills(Pawn pawn)
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp != null)
                comp.missionCount += 20;
        }

        [DebugAction("Military", "Reset Military Pawn", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetPawn(Pawn pawn)
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            bool wasPatrolling = comp.isPatrolling;
            comp.rank = "";
            comp.missionCount = 0;
            comp.isPatrolling = false;
            comp.patrolWaypoints.Clear();

            if (wasPatrolling && pawn.jobs?.curJob?.def == MilitaryJobDefOf.MilitaryPatrol)
                pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
        }

        [DebugAction("Military", "Force Assign Patrol", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ForceAssignPatrol(Pawn pawn)
        {
            if (!pawn.IsColonist || !MilitaryUtility.IsEligible(pawn))
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            comp.patrolWaypoints.Clear();
            Designator_PatrolWaypoint.TargetPawn = pawn;
            Designator_PatrolWaypoint.TargetComp = comp;

            Find.DesignatorManager.Select(new Designator_PatrolWaypoint());
            Messages.Message("Military_WaypointBegin".Translate(), MessageTypeDefOf.SilentInput, false);
        }

        [DebugAction("Military", "List Military Pawns", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ListMilitaryPawns()
        {
            Map map = Find.CurrentMap;
            if (map == null)
                return;

            foreach (Pawn pawn in map.mapPawns.FreeColonists)
            {
                MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
                if (comp == null)
                    continue;
                Log.Message($"[Military] {pawn.LabelShort} — Rank: {comp.rank}, Kills: {comp.missionCount}, Patrolling: {comp.isPatrolling}");
            }
        }

        // ─────────────────────────────────────────────────────────
        // DEV: Quest launchers — instant quest start, skip delays
        // ─────────────────────────────────────────────────────────

        [DebugAction("Military", "DEV: Start Mission 1 Now", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevStartMission1()
        {
            QuestScriptDef questDef = DefDatabase<QuestScriptDef>.GetNamed("Quest_Mission01_CrashAndRegroup", false);
            if (questDef == null)
            {
                Log.Error("[Military] DEV: Quest_Mission01_CrashAndRegroup not found!");
                return;
            }

            Slate slate = new Slate();
            Quest quest = QuestGen.Generate(questDef, slate);
            if (quest == null)
            {
                Log.Error("[Military] DEV: Mission 1 quest generation failed!");
                return;
            }

            quest.Accept(null);
            Find.QuestManager.Add(quest);
            Find.SignalManager.SendSignal(new Signal(quest.InitiateSignal));
            GameComponent_MilitaryManager.Instance?.ScheduleMission1(Find.TickManager.TicksGame);
            Log.Message($"[Military] DEV: Mission 1 started instantly (id={quest.id})");
        }

        [DebugAction("Military", "DEV: Start Mission 2 Now", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevStartMission2()
        {
            QuestScriptDef questDef = DefDatabase<QuestScriptDef>.GetNamed("Quest_Mission02_VanguardsShadow", false);
            if (questDef == null)
            {
                Log.Error("[Military] DEV: Quest_Mission02_VanguardsShadow not found!");
                return;
            }

            Slate slate = new Slate();
            Quest quest = QuestGen.Generate(questDef, slate);
            if (quest == null)
            {
                Log.Error("[Military] DEV: Mission 2 quest generation failed!");
                return;
            }

            // Add quest to manager BEFORE firing InitiateSignal so the quest tab
            // shows instantly when the opening letter appears.
            quest.SetInitiallyAccepted();
            Find.QuestManager.Add(quest);
            Find.SignalManager.SendSignal(new Signal(quest.InitiateSignal));

            // Mark M2 as started in GameComponent so TickMission2 does NOT try to
            // auto-launch another quest after the normal 7-day wait.
            GameComponent_MilitaryManager.Instance?.ForceM2QuestScheduled();
            Log.Message($"[Military] DEV: Mission 2 started instantly (id={quest.id})");
        }

        [DebugAction("Military", "DEV: Complete Mission 1 (fire success signal)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevCompleteMission1()
        {
            // Mark M1 done in GameComponent first to prevent TickMission1 from
            // re-firing the success signal on the next tick.
            GameComponent_MilitaryManager.Instance?.ForceCompleteM1();
            Find.SignalManager.SendSignal(new Signal("PhantomStrike.Mission01.Success"));
            Log.Message("[Military] DEV: Mission 1 success signal fired");
        }

        [DebugAction("Military", "DEV: Complete Mission 2 (fire success signal)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevCompleteMission2()
        {
            Find.SignalManager.SendSignal(new Signal("PhantomStrike.Mission02.TimerComplete"));
            Log.Message("[Military] DEV: Mission 2 timer-complete signal fired");
        }

        [DebugAction("Military", "DEV: Fail Mission 1 (fire fail signal)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevFailMission1()
        {
            Find.SignalManager.SendSignal(new Signal("PhantomStrike.Mission01.Fail"));
            Log.Message("[Military] DEV: Mission 1 fail signal fired");
        }

        [DebugAction("Military", "DEV: Fail Mission 2 (fire fail signal)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevFailMission2()
        {
            Find.SignalManager.SendSignal(new Signal("PhantomStrike.Mission02.Fail"));
            Log.Message("[Military] DEV: Mission 2 fail signal fired");
        }

        [DebugAction("Military", "DEV: Spawn Helix Raid (300pts)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevSpawnHelixRaid300()
        {
            DevSpawnHelixRaidWithPoints(300f);
        }

        [DebugAction("Military", "DEV: Spawn Helix Raid (800pts)", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevSpawnHelixRaid800()
        {
            DevSpawnHelixRaidWithPoints(800f);
        }

        private static void DevSpawnHelixRaidWithPoints(float points)
        {
            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map == null)
            {
                Log.Error("[Military] DEV: No map found for raid!");
                return;
            }

            Faction faction = GameComponent_MilitaryManager.HelixCorpFaction;
            if (faction == null)
            {
                faction = Find.FactionManager.AllFactions
                    .FirstOrDefault(f => f.HostileTo(Faction.OfPlayer) && !f.defeated && f.def.humanlikeFaction);
            }

            if (faction == null)
            {
                Log.Error("[Military] DEV: No hostile faction found!");
                return;
            }

            float minPoints = IncidentDefOf.RaidEnemy.minThreatPoints;
            float finalPoints = System.Math.Max(points, minPoints);

            IncidentParms parms = new IncidentParms();
            parms.target = map;
            parms.faction = faction;
            parms.points = finalPoints;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;

            bool result = IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
            Log.Message($"[Military] DEV: Helix raid spawned — {finalPoints}pts, faction: {faction.Name}, result: {result}");
        }

        [DebugAction("Military", "DEV: Spawn Silas Vane on map", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void DevSpawnVane()
        {
            Map map = Find.CurrentMap;
            if (map == null)
                return;

            PawnKindDef vaneKind = DefDatabase<PawnKindDef>.GetNamed("SilasVane_Guest", false);
            if (vaneKind == null)
            {
                Log.Error("[Military] DEV: SilasVane_Guest not found!");
                return;
            }

            Pawn vane = PawnGenerator.GeneratePawn(vaneKind, Faction.OfPlayer);
            IntVec3 spot = map.Center;
            if (!spot.Walkable(map))
                spot = CellFinder.RandomCell(map);

            GenSpawn.Spawn(vane, spot, map);
            Log.Message($"[Military] DEV: Silas Vane spawned at {spot}");
        }
    }
}
