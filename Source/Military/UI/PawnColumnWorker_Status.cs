using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_Status : PawnColumnWorker
    {
        private static readonly Color PatrolBlue = new Color(0.3f, 0.5f, 0.9f);
        private static readonly Color FightRed = new Color(0.9f, 0.2f, 0.2f);
        private static readonly Color IdleGrey = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color VipGold = new Color(1f, 0.8f, 0f);
        private static readonly Color DefendBlue = new Color(0.2f, 0.6f, 1f);
        private static readonly Color BodyguardGreen = new Color(0.2f, 0.85f, 0.4f);

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
            else
            {
                statusText = "Military_Status_Idle".Translate();
                statusColor = IdleGrey;
            }

            // Build suffix tags for protection roles
            string suffix = null;
            Color suffixColor = Color.white;
            if (comp.vipBodyguards.Count > 0)
            {
                suffix = "Military_Status_VIP".Translate();
                suffixColor = VipGold;
            }
            else if (comp.isDefending)
            {
                suffix = "Military_Status_Defending".Translate();
                suffixColor = DefendBlue;
            }
            else if (comp.bodyguardTarget != null)
            {
                suffix = "Military_Status_Bodyguard".Translate();
                suffixColor = BodyguardGreen;
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
