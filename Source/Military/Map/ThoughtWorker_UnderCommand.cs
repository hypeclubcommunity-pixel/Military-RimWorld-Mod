using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_UnderCommand : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!MilitaryUtility.IsEligible(p) || !p.Spawned || p.Map == null)
                return ThoughtState.Inactive;

            MilitaryStatComp comp = MilitaryUtility.GetComp(p);
            if (comp == null || string.IsNullOrEmpty(comp.squadId) || comp.isSquadLeader)
                return ThoughtState.Inactive;

            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            SquadData squad = manager?.GetSquadOf(p);
            Pawn leader = squad?.GetLeader(p.Map);
            if (!MilitaryUtility.IsLivePlayerColonistOnMap(leader, p.Map))
                return ThoughtState.Inactive;
            if (leader.InMentalState)
                return ThoughtState.Inactive;
            if (!Patches.RankStatPatch.IsNearOwnSquadLeader(p))
                return ThoughtState.Inactive;

            return ThoughtState.ActiveDefault;
        }
    }
}
