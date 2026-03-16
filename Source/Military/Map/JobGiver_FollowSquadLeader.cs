using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_FollowSquadLeader : ThinkNode_JobGiver
    {
        private const float MaxFollowDistance = 10f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.IsFreeColonist || pawn.Drafted || !pawn.Spawned || pawn.Downed)
                return null;

            if (!MilitaryUtility.IsEligible(pawn))
                return null;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.squadId) || comp.isSquadLeader)
                return null;

            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            if (manager == null)
                return null;

            SquadData squad = manager.GetSquadOf(pawn);
            if (squad == null)
                return null;

            Pawn leader = squad.GetLeader(pawn.Map);
            if (leader == null || leader.Downed || !leader.Spawned || leader.Map != pawn.Map)
                return null;

            if (pawn.Position.InHorDistOf(leader.Position, MaxFollowDistance))
                return null;

            Job job = JobMaker.MakeJob(JobDefOf.Follow, leader);
            job.expiryInterval = 200;
            job.checkOverrideOnExpire = true;
            return job;
        }
    }
}
