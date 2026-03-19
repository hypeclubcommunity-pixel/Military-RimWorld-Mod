using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_BodyguardFollow : ThinkNode_JobGiver
    {
        private const int FollowExpiryTicks = 1800;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.IsFreeColonist || pawn.Drafted || !pawn.Spawned || pawn.Downed)
                return null;

            if (!MilitaryUtility.IsEligible(pawn))
                return null;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || comp.bodyguardTarget == null)
                return null;

            Pawn vip = comp.bodyguardTarget;
            if (vip.Downed || !vip.Spawned || vip.Map != pawn.Map)
                return null;

            if (pawn.CurJobDef == JobDefOf.Follow && pawn.CurJob?.targetA.Thing == vip)
                return null;

            Job job = JobMaker.MakeJob(JobDefOf.Follow, vip);
            job.expiryInterval = FollowExpiryTicks;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}
