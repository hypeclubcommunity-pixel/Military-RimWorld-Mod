using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_BodyguardFollow : ThinkNode_JobGiver
    {
        private const float MaxFollowDistance = 8f;

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

            if (pawn.Position.InHorDistOf(vip.Position, MaxFollowDistance))
                return null;

            Job job = JobMaker.MakeJob(JobDefOf.Follow, vip);
            job.expiryInterval = 180;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}
