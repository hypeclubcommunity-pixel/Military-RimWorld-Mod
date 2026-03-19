using HarmonyLib;
using Verse;

namespace Military.Patches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
    public static class PawnDamageTracker_Patch
    {
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            if (__instance == null || totalDamageDealt <= 0f)
                return;

            Pawn instigator = dinfo.Instigator as Pawn;
            if (instigator == null || __instance.Map == null || Find.TickManager == null)
                return;

            MilitaryResponseSystem responseSystem = __instance.Map.GetComponent<MilitaryResponseSystem>();
            responseSystem?.RecordHostileDamage(__instance, instigator, Find.TickManager.TicksGame);
        }
    }
}
