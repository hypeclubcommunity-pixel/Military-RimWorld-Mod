using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_RespectsCommand : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (pawn == null || other == null || pawn == other)
                return ThoughtState.Inactive;
            if (!MilitaryUtility.IsLivePlayerColonistOnMap(pawn, pawn.Map))
                return ThoughtState.Inactive;
            if (!MilitaryUtility.IsLivePlayerColonistOnMap(other, pawn.Map))
                return ThoughtState.Inactive;

            MilitaryStatComp pawnComp = MilitaryUtility.GetComp(pawn);
            MilitaryStatComp otherComp = MilitaryUtility.GetComp(other);
            if (pawnComp == null || otherComp == null)
                return ThoughtState.Inactive;
            if (string.IsNullOrEmpty(pawnComp.squadId) || pawnComp.isSquadLeader)
                return ThoughtState.Inactive;
            if (pawnComp.squadId != otherComp.squadId || !otherComp.isSquadLeader)
                return ThoughtState.Inactive;

            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            SquadData squad = manager?.GetSquadOf(pawn);
            if (squad == null || squad.leaderPawnId != other.thingIDNumber)
                return ThoughtState.Inactive;

            return ThoughtState.ActiveDefault;
        }
    }
}
