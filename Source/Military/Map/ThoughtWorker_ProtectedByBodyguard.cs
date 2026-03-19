using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_ProtectedByBodyguard : ThoughtWorker
    {
        private const float ProtectRadius = 12f;

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!MilitaryUtility.IsEligible(p) || !p.Spawned || p.Map == null)
                return ThoughtState.Inactive;

            MilitaryStatComp comp = MilitaryUtility.GetComp(p);
            if (comp == null || comp.vipBodyguards == null || comp.vipBodyguards.Count == 0)
                return ThoughtState.Inactive;

            for (int i = 0; i < comp.vipBodyguards.Count; i++)
            {
                Pawn bodyguard = comp.vipBodyguards[i];
                if (!MilitaryUtility.IsLivePlayerColonistOnMap(bodyguard, p.Map))
                    continue;

                MilitaryStatComp bgComp = MilitaryUtility.GetComp(bodyguard);
                if (bgComp == null || bgComp.bodyguardTarget != p)
                    continue;

                if (!bodyguard.Position.InHorDistOf(p.Position, ProtectRadius))
                    continue;

                return ThoughtState.ActiveDefault;
            }

            return ThoughtState.Inactive;
        }
    }
}
