using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_DefendArea : ThinkNode_JobGiver
    {
        private const int ThreatScanRadius = 5;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.IsFreeColonist || pawn.Drafted || !pawn.Spawned || pawn.Downed)
                return null;

            if (!MilitaryUtility.IsEligible(pawn))
                return null;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || !comp.isDefending || comp.defendArea.Count == 0)
                return null;

            // Compute bounding rect from defend area cells once — O(n).
            // All subsequent containment/proximity checks are then O(1).
            List<IntVec3> area = comp.defendArea;
            int minX = area[0].x, maxX = minX;
            int minZ = area[0].z, maxZ = minZ;
            for (int i = 1; i < area.Count; i++)
            {
                int cx = area[i].x, cz = area[i].z;
                if (cx < minX) minX = cx; else if (cx > maxX) maxX = cx;
                if (cz < minZ) minZ = cz; else if (cz > maxZ) maxZ = cz;
            }
            CellRect defendRect = CellRect.FromLimits(new IntVec3(minX, 0, minZ), new IntVec3(maxX, 0, maxZ));
            CellRect threatRect = defendRect.ExpandedBy(ThreatScanRadius);

            // Engage threats first (priority over repositioning).
            IReadOnlyList<Pawn> allPawns = pawn.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn candidate = allPawns[i];
                if (candidate == null || !candidate.RaceProps.Humanlike || candidate.Downed || !candidate.Spawned)
                    continue;

                if (!candidate.HostileTo(Faction.OfPlayer))
                    continue;

                if (!threatRect.Contains(candidate.Position))
                    continue;

                if (!GenSight.LineOfSight(pawn.Position, candidate.Position, pawn.Map))
                    continue;

                if (pawn.equipment?.Primary?.def.IsRangedWeapon ?? false)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, candidate);
                    job.locomotionUrgency = LocomotionUrgency.Jog;
                    return job;
                }

                return JobMaker.MakeJob(JobDefOf.AttackMelee, candidate);
            }

            // Already inside defend area — nothing to do.
            if (defendRect.Contains(pawn.Position))
                return null;

            // Find nearest walkable cell in the defend area.
            IntVec3 nearest = IntVec3.Invalid;
            float bestDistSq = float.MaxValue;
            for (int i = 0; i < area.Count; i++)
            {
                IntVec3 cell = area[i];
                if (!cell.Walkable(pawn.Map))
                    continue;

                float distSq = (cell - pawn.Position).LengthHorizontalSquared;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    nearest = cell;
                }
            }

            if (!nearest.IsValid)
                return null;

            Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, nearest);
            gotoJob.expiryInterval = 300;
            return gotoJob;
        }
    }
}
