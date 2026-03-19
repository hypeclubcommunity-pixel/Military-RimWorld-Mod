using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_Status : PawnColumnWorker
    {
        private static readonly Color PatrolBlue = MilitaryTheme.Patrol;
        private static readonly Color FightRed = MilitaryTheme.Fighting;
        private static readonly Color IdleGrey = MilitaryTheme.Idle;
        private static readonly Color VipGold = MilitaryTheme.Vip;
        private static readonly Color DefendBlue = MilitaryTheme.Defend;
        private static readonly Color BodyguardGreen = MilitaryTheme.Bodyguard;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            string statusText;
            Color statusColor;

            var curJobDef = pawn.CurJobDef;
            if (curJobDef == JobDefOf.AttackMelee || curJobDef == JobDefOf.AttackStatic)
            {
                statusText = "Military_Status_Fighting".Translate();
                statusColor = FightRed;
            }
            else if (comp.isPatrolling)
            {
                statusText = "Military_Status_Patrolling".Translate();
                statusColor = PatrolBlue;
            }
            else if (comp.isDefending)
            {
                statusText = "Military_Status_Defending".Translate();
                statusColor = DefendBlue;
            }
            else if (comp.bodyguardTarget != null)
            {
                statusText = "Military_Status_Bodyguard".Translate();
                statusColor = BodyguardGreen;
            }
            else
            {
                statusText = "Military_Status_Idle".Translate();
                statusColor = IdleGrey;
            }

            // VIP is the only supplemental tag we keep beside the primary duty status.
            string suffix = null;
            Color suffixColor = MilitaryTheme.TextPrimary;
            if (comp.vipBodyguards.Count > 0)
            {
                suffix = "Military_Status_VIP".Translate();
                suffixColor = VipGold;
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            if (suffix == null)
            {
                GUI.color = statusColor;
                Widgets.Label(rect, statusText);
            }
            else
            {
                // Split rect: status on left half, suffix on right half
                Rect leftRect = new Rect(rect.x, rect.y, rect.width * 0.5f, rect.height);
                Rect rightRect = new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.5f, rect.height);
                GUI.color = statusColor;
                Widgets.Label(leftRect, statusText);
                GUI.color = suffixColor;
                Widgets.Label(rightRect, suffix);
            }

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override int GetMinWidth(PawnTable table) => 80;
        public override int GetOptimalWidth(PawnTable table) => 80;

        public override int Compare(Pawn a, Pawn b) => 0;
    }
}
