using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace Military.Patches
{
    public static class RankStatPatch
    {
        // Cached nearby commander data per pawn, refreshed only on IsHashIntervalTick(120).
        private static readonly Dictionary<int, CachedAura> auraCache = new Dictionary<int, CachedAura>();

        private struct CachedAura
        {
            public bool nearSergeant;
            public bool nearLieutenant;
            public bool initialized;
        }

        public static void EnsureStatPartsInstalled()
        {
            AddPartIfMissing(StatDefOf.ShootingAccuracyPawn, typeof(StatPart_MilitaryShooting));
            AddPartIfMissing(StatDefOf.MoveSpeed, typeof(StatPart_MilitaryMoveSpeed));
            AddPartIfMissing(StatDefOf.AimingDelayFactor, typeof(StatPart_MilitaryAimingDelay));
        }

        private static void AddPartIfMissing(StatDef stat, Type partType)
        {
            if (stat == null)
                return;

            if (stat.parts == null)
                stat.parts = new List<StatPart>();

            for (int i = 0; i < stat.parts.Count; i++)
            {
                if (stat.parts[i]?.GetType() == partType)
                    return;
            }

            StatPart part = Activator.CreateInstance(partType) as StatPart;
            if (part == null)
                return;

            stat.parts.Add(part);
        }

        public static bool TryGetRankedColonist(StatRequest req, out Pawn pawn, out MilitaryStatComp comp, out int rankIndex)
        {
            pawn = null;
            comp = null;
            rankIndex = -1;

            if (!req.HasThing)
                return false;

            pawn = req.Thing as Pawn;
            if (pawn == null || !pawn.IsColonist || pawn.Faction != Faction.OfPlayer)
                return false;

            if (!MilitaryUtility.IsEligible(pawn))
                return false;

            comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return false;

            rankIndex = MilitaryRanks.All.IndexOf(comp.rank);
            if (rankIndex < 0)
                return false;

            return true;
        }

        public static bool IsNearSergeant(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned)
                return false;
            return GetCachedAura(pawn).nearSergeant;
        }

        public static bool IsNearLieutenant(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned)
                return false;
            return GetCachedAura(pawn).nearLieutenant;
        }

        private static CachedAura GetCachedAura(Pawn pawn)
        {
            int pawnId = pawn.thingIDNumber;
            bool hasCached = auraCache.TryGetValue(pawnId, out CachedAura cached);
            if (hasCached && cached.initialized && !pawn.IsHashIntervalTick(120))
                return cached;

            CachedAura result = default;
            result.initialized = true;

            if (pawn.Map == null)
            {
                auraCache[pawnId] = result;
                return result;
            }

            List<Pawn> mapPawns = pawn.Map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < mapPawns.Count; i++)
            {
                Pawn other = mapPawns[i];
                if (other == pawn || !other.Spawned)
                    continue;

                MilitaryStatComp otherComp = MilitaryUtility.GetComp(other);
                if (otherComp == null || string.IsNullOrEmpty(otherComp.rank))
                    continue;

                if (!pawn.Position.InHorDistOf(other.Position, 15f))
                    continue;

                if (otherComp.rank == "Sergeant")
                    result.nearSergeant = true;
                else if (otherComp.rank == "Lieutenant")
                    result.nearLieutenant = true;

                if (result.nearSergeant && result.nearLieutenant)
                    break;
            }

            auraCache[pawnId] = result;
            return result;
        }
    }

    public class StatPart_MilitaryShooting : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!RankStatPatch.TryGetRankedColonist(req, out Pawn pawn, out _, out int rankIndex))
                return;

            // Private and above: +0.03 ShootingAccuracyPawn
            if (rankIndex >= 1)
                val += 0.03f;

            // Sergeant aura within 15 cells: +0.03 ShootingAccuracyPawn
            if (RankStatPatch.IsNearSergeant(pawn))
                val += 0.03f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!RankStatPatch.TryGetRankedColonist(req, out Pawn pawn, out _, out int rankIndex))
                return null;

            StringBuilder sb = new StringBuilder();
            if (rankIndex >= 1)
                sb.Append("Military_StatBonus_Private".Translate());
            if (RankStatPatch.IsNearSergeant(pawn))
            {
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.Append("Military_StatBonus_Sergeant".Translate());
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }
    }

    public class StatPart_MilitaryMoveSpeed : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!RankStatPatch.TryGetRankedColonist(req, out _, out _, out int rankIndex))
                return;

            // Corporal and above: +0.05 MoveSpeed
            if (rankIndex >= 2)
                val += 0.05f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!RankStatPatch.TryGetRankedColonist(req, out _, out _, out int rankIndex))
                return null;
            return rankIndex >= 2 ? "Military_StatBonus_Corporal".Translate() : null;
        }
    }

    public class StatPart_MilitaryAimingDelay : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!RankStatPatch.TryGetRankedColonist(req, out Pawn pawn, out _, out _))
                return;

            // Lieutenant aura within 15 cells: -0.05 AimingDelayFactor
            if (RankStatPatch.IsNearLieutenant(pawn))
                val -= 0.05f;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!RankStatPatch.TryGetRankedColonist(req, out Pawn pawn, out _, out _))
                return null;
            return RankStatPatch.IsNearLieutenant(pawn) ? "Military_StatBonus_Lieutenant".Translate() : null;
        }
    }
}
