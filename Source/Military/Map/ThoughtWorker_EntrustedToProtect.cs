using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_EntrustedToProtect : ThoughtWorker
    {
        private const float TrustRadius = 12f;

        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (pawn == null || other == null || pawn == other)
                return ThoughtState.Inactive;
            if (!MilitaryUtility.IsLivePlayerColonistOnMap(pawn, pawn.Map))
                return ThoughtState.Inactive;
            if (!MilitaryUtility.IsLivePlayerColonistOnMap(other, pawn.Map))
                return ThoughtState.Inactive;
            if (!MilitaryUtility.IsAssignedBodyguardPair(pawn, other))
                return ThoughtState.Inactive;
            if (!pawn.Position.InHorDistOf(other.Position, TrustRadius))
                return ThoughtState.Inactive;

            return ThoughtState.ActiveDefault;
        }
    }
}
