using System.Collections.Generic;
using LudeonTK;
using RimWorld;
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
    }
}
