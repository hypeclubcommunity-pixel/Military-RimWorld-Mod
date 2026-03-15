using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_MilitaryAutoAttack : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.IsFreeColonist || pawn.Drafted || !pawn.Spawned || pawn.Downed)
                return null;

            if (!MilitaryUtility.IsEligible(pawn))
                return null;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return null;

            // Only patrol-assigned colonists engage autonomously.
            if (!comp.isPatrolling)
                return null;

            IAttackTarget target = AttackTargetFinder.BestAttackTarget(
                pawn,
                TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat,
                validator: null,
                minDist: 0f,
                maxDist: 20f);

            if (target == null)
                return null;

            // Break patrol state but preserve waypoints — watchdog will resume after combat.
            comp.isPatrolling = false;

            if (pawn.equipment?.Primary?.def.IsRangedWeapon ?? false)
            {
                Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, target.Thing);
                job.locomotionUrgency = LocomotionUrgency.Jog;
                return job;
            }

            return JobMaker.MakeJob(JobDefOf.AttackMelee, target.Thing);
        }
    }
}
