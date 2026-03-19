using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_ProtectiveDuty : ThoughtWorker
    {
        private const float ProtectRadius = 12f;

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!MilitaryUtility.IsEligible(p) || !p.Spawned || p.Map == null)
                return ThoughtState.Inactive;

            MilitaryStatComp comp = MilitaryUtility.GetComp(p);
            Pawn vip = comp?.bodyguardTarget;
            if (!MilitaryUtility.IsLivePlayerColonistOnMap(vip, p.Map))
                return ThoughtState.Inactive;

            if (!p.Position.InHorDistOf(vip.Position, ProtectRadius))
                return ThoughtState.Inactive;

            return ThoughtState.ActiveDefault;
        }
    }
}
