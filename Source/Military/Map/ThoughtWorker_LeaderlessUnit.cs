using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_LeaderlessUnit : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!MilitaryUtility.IsEligible(p) || !MilitaryUtility.IsLivePlayerColonistOnMap(p, p.Map))
                return ThoughtState.Inactive;

            MilitaryStatComp comp = MilitaryUtility.GetComp(p);
            if (comp == null || string.IsNullOrEmpty(comp.squadId) || comp.isSquadLeader)
                return ThoughtState.Inactive;

            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            SquadData squad = manager?.GetSquadOf(p);
            if (squad == null)
                return ThoughtState.Inactive;

            Pawn leader = MilitaryUtility.FindPawnGlobal(squad.leaderPawnId);
            if (leader == null
                || leader.Dead
                || leader.Downed
                || leader.InMentalState
                || !MilitaryUtility.IsEligible(leader)
                || leader.Faction != Faction.OfPlayer
                || !leader.IsColonist)
            {
                return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }
    }
}
