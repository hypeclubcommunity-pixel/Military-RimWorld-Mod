using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_PatrolIcon : PawnColumnWorker
    {
        private static readonly Color ActiveColor = MilitaryTheme.Patrol;
        private static readonly Color InactiveColor = MilitaryTheme.Disabled;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return;

            bool hasWaypoints = comp.patrolWaypoints != null && comp.patrolWaypoints.Count >= 2;
            bool isPatrolling = comp.isPatrolling;

            float iconSize = 24f;
            Rect iconRect = new Rect(
                rect.x + (rect.width - iconSize) / 2f,
                rect.y + (rect.height - iconSize) / 2f,
                iconSize,
                iconSize
            );

            GUI.color = isPatrolling ? ActiveColor : InactiveColor;
            GUI.DrawTexture(iconRect, MilitaryTextures.PatrolColumn);
            GUI.color = Color.white;

            if (hasWaypoints)
            {
                if (Widgets.ButtonInvisible(iconRect))
                {
                    if (isPatrolling)
                    {
                        comp.isPatrolling = false;
                        comp.patrolWaypoints?.Clear();
                        pawn.jobs?.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                    }
                    else if (hasWaypoints)
                    {
                        comp.isPatrolling = true;
                        // The CompTick will dispatch the patrol job on the next tick
                    }
                }

                string tip = isPatrolling
                    ? "Military_Patrol_StopTip".Translate()
                    : "Military_Patrol_StartTip".Translate();
                TooltipHandler.TipRegion(iconRect, tip);
            }
            else
            {
                TooltipHandler.TipRegion(iconRect, "Military_Patrol_NoWaypointsTip".Translate());
            }
        }

        public override int GetMinWidth(PawnTable table) => 50;
        public override int GetOptimalWidth(PawnTable table) => 50;

        public override int Compare(Pawn a, Pawn b)
        {
            bool patrolA = MilitaryUtility.GetComp(a)?.isPatrolling ?? false;
            bool patrolB = MilitaryUtility.GetComp(b)?.isPatrolling ?? false;
            return patrolA.CompareTo(patrolB);
        }
    }
}
