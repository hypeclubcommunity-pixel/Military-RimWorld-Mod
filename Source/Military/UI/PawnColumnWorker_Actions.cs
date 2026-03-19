using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_Actions : PawnColumnWorker
    {
        private const float ButtonWidth = 70f;
        private const float ButtonHeight = 24f;
        private const float Spacing = 8f;

        private static readonly Color PromoteGreenTint = MilitaryTheme.Promote;
        private static readonly Color DisabledTint = MilitaryTheme.Disabled;
        private static readonly Color DemoteRedTint = MilitaryTheme.Demote;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return;

            string currentRank = comp.rank;
            string nextRank = MilitaryRanks.Next(currentRank);
            string prevRank = MilitaryRanks.Previous(currentRank);

            bool isMaxRank = currentRank == MilitaryRanks.All[MilitaryRanks.All.Count - 1];
            bool isMinRank = currentRank == MilitaryRanks.All[0];
            bool canPromote = !isMaxRank && MilitaryRanks.IsEligibleForRank(nextRank, comp.missionCount);
            bool canDemote = !isMinRank;

            float totalWidth = (ButtonWidth * 2f) + Spacing;
            float startX = rect.x + (rect.width - totalWidth) / 2f;
            float startY = rect.y + (rect.height - ButtonHeight) / 2f;

            Rect promoteRect = new Rect(startX, startY, ButtonWidth, ButtonHeight);
            Rect demoteRect = new Rect(startX + ButtonWidth + Spacing, startY, ButtonWidth, ButtonHeight);

            // Promote button
            GUI.color = canPromote ? PromoteGreenTint : DisabledTint;
            if (Widgets.ButtonText(promoteRect, "Military_Gizmo_Promote".Translate()) && canPromote)
            {
                MilitaryUtility.SetRank(pawn, nextRank);
                MilitaryUtility.SendPromotionLetter(pawn, nextRank);
            }
            GUI.color = Color.white;

            if (isMaxRank)
                TooltipHandler.TipRegion(promoteRect, "Military_DisabledMaxRank".Translate());
            else if (!canPromote)
                TooltipHandler.TipRegion(promoteRect, "Military_RequiresKills".Translate(MilitaryRanks.KillThresholds[nextRank]));
            else
                TooltipHandler.TipRegion(promoteRect, "Military_Gizmo_PromoteDesc".Translate(MilitaryRanks.TranslatedName(currentRank), MilitaryRanks.TranslatedName(nextRank)));

            // Demote button
            GUI.color = canDemote ? DemoteRedTint : DisabledTint;
            if (Widgets.ButtonText(demoteRect, "Military_Gizmo_Demote".Translate()) && canDemote)
            {
                MilitaryUtility.SetRank(pawn, prevRank);
            }
            GUI.color = Color.white;

            if (!canDemote)
                TooltipHandler.TipRegion(demoteRect, "Military_DisabledMinRank".Translate());
            else
                TooltipHandler.TipRegion(demoteRect, "Military_Gizmo_DemoteDesc".Translate(MilitaryRanks.TranslatedName(currentRank), MilitaryRanks.TranslatedName(prevRank)));
        }

        public override int GetMinWidth(PawnTable table) => 160;
        public override int GetOptimalWidth(PawnTable table) => 160;

        public override int Compare(Pawn a, Pawn b) => 0;
    }
}
