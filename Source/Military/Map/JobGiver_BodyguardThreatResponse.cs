using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_BodyguardThreatResponse : ThinkNode_JobGiver
    {
        private const float ThreatScanRadius = 15f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.IsFreeColonist || pawn.Drafted || !pawn.Spawned || pawn.Downed)
                return null;

            if (!MilitaryUtility.IsEligible(pawn))
                return null;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || comp.bodyguardTargetId == -1)
                return null;

            Pawn vip = MilitaryUtility.FindPawnGlobal(comp.bodyguardTargetId);
            if (vip == null || vip.Downed || !vip.Spawned || vip.Map != pawn.Map)
                return null;

            IReadOnlyList<Pawn> allPawns = pawn.Map.mapPawns.AllPawnsSpawned;
            Pawn threat = null;
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn candidate = allPawns[i];
                if (candidate == null || !candidate.RaceProps.Humanlike || candidate.Downed || !candidate.Spawned)
                    continue;

                if (!candidate.HostileTo(Faction.OfPlayer))
                    continue;

                if (!candidate.Position.InHorDistOf(vip.Position, ThreatScanRadius))
                    continue;

                if (!GenSight.LineOfSight(pawn.Position, candidate.Position, pawn.Map))
                    continue;

                threat = candidate;
                break;
            }

            if (threat == null)
                return null;

            if (pawn.equipment?.Primary?.def.IsRangedWeapon ?? false)
            {
                Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, threat);
                job.locomotionUrgency = LocomotionUrgency.Jog;
                return job;
            }

            return JobMaker.MakeJob(JobDefOf.AttackMelee, threat);
        }
    }
}
