using HarmonyLib;
using RimWorld;
using Verse;

namespace Military.Patches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill), new[] { typeof(DamageInfo?), typeof(Hediff) })]
    public static class KillTracker_Patch
    {
        public static void Postfix(Pawn __instance, DamageInfo? dinfo)
        {
            if (__instance == null)
                return;

            // Dead pawns may not remain HostileTo(player), so rely on stable faction checks.
            if (__instance.Faction == null
                || __instance.Faction == Faction.OfPlayer
                || !__instance.Faction.HostileTo(Faction.OfPlayer))
                return;

            // Resolve the instigator — prefer dinfo, fall back to meleeThreat on the killed pawn
            Pawn instigator = dinfo?.Instigator as Pawn;
            if (instigator == null || instigator.Faction != Faction.OfPlayer || !instigator.IsColonist)
                instigator = __instance.mindState?.meleeThreat;

            if (instigator == null || instigator.Faction != Faction.OfPlayer || !instigator.IsColonist)
                return;

            // Check the instigator is eligible for military service
            bool eligible = MilitaryUtility.IsEligible(instigator);
            if (!eligible)
                return;

            // Increment the mission count on the instigator's MilitaryStatComp
            var comp = MilitaryUtility.GetComp(instigator);
            if (comp == null)
                return;

            string currentRank = comp.rank;
            if (string.IsNullOrEmpty(currentRank))
                return;

            int oldKills = comp.missionCount;
            comp.missionCount++;
            int newKills = comp.missionCount;

            // Check if a new rank threshold was crossed
            string nextRank = MilitaryRanks.Next(currentRank);
            if (nextRank == currentRank)
                return; // Already max rank

            int threshold = MilitaryRanks.KillThresholds.TryGetValue(nextRank, out int t) ? t : int.MaxValue;
            if (oldKills < threshold && newKills >= threshold)
            {
                MilitaryUtility.SendEligibilityLetter(instigator, nextRank);
            }
        }
    }
}
