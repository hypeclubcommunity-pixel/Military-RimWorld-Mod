using RimWorld;
using Verse;

namespace Military
{
    public class ThoughtWorker_RankMood : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!MilitaryUtility.IsEligible(p))
                return ThoughtState.Inactive;

            MilitaryStatComp comp = MilitaryUtility.GetComp(p);
            if (comp == null)
                return ThoughtState.Inactive;

            switch (comp.rank)
            {
                case "Private":
                    return ThoughtState.ActiveAtStage(0);
                case "Corporal":
                    return ThoughtState.ActiveAtStage(1);
                case "Sergeant":
                    return ThoughtState.ActiveAtStage(2);
                case "Lieutenant":
                    return ThoughtState.ActiveAtStage(3);
                default:
                    return ThoughtState.Inactive;
            }
        }
    }
}
