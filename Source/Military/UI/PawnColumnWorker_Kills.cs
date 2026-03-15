using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_Kills : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, comp.missionCount.ToString());
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override int GetMinWidth(PawnTable table) => 60;
        public override int GetOptimalWidth(PawnTable table) => 60;

        public override int Compare(Pawn a, Pawn b)
        {
            MilitaryStatComp compA = MilitaryUtility.GetComp(a);
            MilitaryStatComp compB = MilitaryUtility.GetComp(b);
            int countA = compA?.missionCount ?? 0;
            int countB = compB?.missionCount ?? 0;
            return countA.CompareTo(countB);
        }
    }
}
