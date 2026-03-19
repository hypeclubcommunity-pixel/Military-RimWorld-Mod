using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Military
{
    internal static class PawnRemovalUtility
    {
        public static void TryHandlePawnRemoved(Pawn pawn)
        {
            if (Faction.OfPlayerSilentFail == null) return;
            if (pawn?.Faction != Faction.OfPlayerSilentFail) return;

            if (pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            if (!string.IsNullOrEmpty(comp.squadId))
                GameComponent_MilitaryManager.Instance?.OnPawnRemoved(pawn);

            MilitaryUtility.OnVipRemoved(pawn);
            MilitaryUtility.ClearBodyguard(pawn);
            MilitaryUtility.ClearDefendArea(pawn);
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill), new[] { typeof(DamageInfo?), typeof(Hediff) })]
    public static class PawnRemoval_Patch_PawnKill
    {
        public static void Postfix(Pawn __instance)
        {
            if (__instance != null && __instance.Dead)
            {
                TryApplySquadDeathMemories(__instance);
                TryApplyVipFailureMemories(__instance);
            }

            PawnRemovalUtility.TryHandlePawnRemoved(__instance);
        }

        private static void TryApplySquadDeathMemories(Pawn deadPawn)
        {
            if (deadPawn == null || deadPawn.Faction != Faction.OfPlayerSilentFail || deadPawn.MapHeld == null)
                return;

            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            SquadData squad = manager?.GetSquadOf(deadPawn);
            if (squad == null)
                return;

            Map map = deadPawn.MapHeld;
            bool wasLeader = squad.leaderPawnId == deadPawn.thingIDNumber;

            Pawn leader = squad.GetLeader(map);
            List<Pawn> members = squad.GetMembers(map);

            if (wasLeader)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    Pawn member = members[i];
                    if (!MilitaryUtility.CanReceivePlayerColonistMemoryOnMap(member, map))
                        continue;
                    MilitaryUtility.TryGainMemory(member, MilitaryThoughtDefOf.Military_LostSquadLeader);
                }
                return;
            }

            if (MilitaryUtility.CanReceivePlayerColonistMemoryOnMap(leader, map))
                MilitaryUtility.TryGainMemory(leader, MilitaryThoughtDefOf.Military_LostSquadmate);

            for (int i = 0; i < members.Count; i++)
            {
                Pawn member = members[i];
                if (member == deadPawn)
                    continue;
                if (!MilitaryUtility.CanReceivePlayerColonistMemoryOnMap(member, map))
                    continue;

                MilitaryUtility.TryGainMemory(member, MilitaryThoughtDefOf.Military_LostSquadmate);
            }
        }

        private static void TryApplyVipFailureMemories(Pawn deadPawn)
        {
            if (deadPawn == null || deadPawn.Faction != Faction.OfPlayerSilentFail)
                return;

            MilitaryStatComp vipComp = MilitaryUtility.GetComp(deadPawn);
            if (vipComp == null || vipComp.vipBodyguards == null || vipComp.vipBodyguards.Count == 0)
                return;

            Map map = deadPawn.MapHeld;
            for (int i = 0; i < vipComp.vipBodyguards.Count; i++)
            {
                Pawn bodyguard = vipComp.vipBodyguards[i];
                if (!MilitaryUtility.CanReceivePlayerColonistMemoryOnMap(bodyguard, map))
                    continue;

                MilitaryStatComp bgComp = MilitaryUtility.GetComp(bodyguard);
                if (bgComp == null || bgComp.bodyguardTarget != deadPawn)
                    continue;

                MilitaryUtility.TryGainMemory(bodyguard, MilitaryThoughtDefOf.Military_FailedToProtectVip);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Destroy), new[] { typeof(DestroyMode) })]
    public static class PawnRemoval_Patch_PawnDestroy
    {
        public static void Postfix(Pawn __instance)
        {
            PawnRemovalUtility.TryHandlePawnRemoved(__instance);
        }
    }

    [HarmonyPatch]
    public static class PawnRemoval_Patch_FactionUtilityNotifyPawnLost
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(FactionUtility), "Notify_PawnLost");
        }

        private static bool Prepare()
        {
            return TargetMethod() != null;
        }

        public static void Postfix(object __0)
        {
            if (__0 is Pawn pawn)
                PawnRemovalUtility.TryHandlePawnRemoved(pawn);
        }
    }
}
