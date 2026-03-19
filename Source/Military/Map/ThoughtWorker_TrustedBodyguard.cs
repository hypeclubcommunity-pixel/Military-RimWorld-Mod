using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_TrustedBodyguard : ThoughtWorker
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
            if (!MilitaryUtility.IsAssignedBodyguardPair(other, pawn))
                return ThoughtState.Inactive;
            if (!other.Position.InHorDistOf(pawn.Position, TrustRadius))
                return ThoughtState.Inactive;

            return ThoughtState.ActiveDefault;
        }
    }
}
