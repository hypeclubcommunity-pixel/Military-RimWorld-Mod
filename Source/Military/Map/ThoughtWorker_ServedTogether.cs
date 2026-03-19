using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_ServedTogether : ThoughtWorker
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
            if (string.IsNullOrEmpty(pawnComp.squadId) || pawnComp.squadId != otherComp.squadId)
                return ThoughtState.Inactive;

            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            SquadData squad = manager?.GetSquadOf(pawn);
            if (squad == null || squad.squadId != otherComp.squadId)
                return ThoughtState.Inactive;
            if (squad.leaderPawnId != other.thingIDNumber
                && (squad.memberPawnIds == null || !squad.memberPawnIds.Contains(other.thingIDNumber)))
            {
                return ThoughtState.Inactive;
            }

            return ThoughtState.ActiveDefault;
        }
    }
}
