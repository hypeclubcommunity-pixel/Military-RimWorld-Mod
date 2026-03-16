using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_SquadAttackTarget : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.IsFreeColonist || !pawn.Drafted || !pawn.Spawned || pawn.Downed)
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
            if (leader == null || !leader.Drafted || !leader.Spawned || leader.Downed)
                return null;

            Pawn target = leader.CurJob?.targetA.Thing as Pawn;
            if (target == null || target.Dead || target.Downed || !target.Spawned)
                return null;

            float range = pawn.equipment?.Primary?.def.Verbs?.FirstOrDefault()?.range ?? 25f;
            if (!pawn.Position.InHorDistOf(target.Position, range))
                return null;

            if (!GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map))
                return null;

            if (pawn.equipment?.Primary?.def.IsRangedWeapon ?? false)
            {
                Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
                job.locomotionUrgency = LocomotionUrgency.Jog;
                return job;
            }

            return JobMaker.MakeJob(JobDefOf.AttackMelee, target);
        }
    }
}
