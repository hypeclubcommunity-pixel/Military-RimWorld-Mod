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
            PawnRemovalUtility.TryHandlePawnRemoved(__instance);
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
