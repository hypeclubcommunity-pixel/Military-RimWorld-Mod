using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_RankProgress : PawnColumnWorker
    {
        private static readonly Color BarFillColor = new Color(0.35f, 0.65f, 0.35f);
        private static readonly Color BarBgColor = new Color(0.15f, 0.15f, 0.15f);
        private static readonly Texture2D BarFillTexture = SolidColorMaterials.NewSolidColorTexture(BarFillColor);
        private static readonly Texture2D BarBgTexture = SolidColorMaterials.NewSolidColorTexture(BarBgColor);

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return;

            string currentRank = comp.rank;
            bool isMaxRank = currentRank == MilitaryRanks.All[MilitaryRanks.All.Count - 1];

            if (isMaxRank)
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = new Color(0.9f, 0.7f, 0.1f);
                Widgets.Label(rect, "MAX");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            string nextRank = MilitaryRanks.Next(currentRank);
            int currentKills = comp.missionCount;
            int nextThreshold = MilitaryRanks.KillThresholds[nextRank];
            int prevThreshold = MilitaryRanks.KillThresholds[currentRank];

            float fillPercent = nextThreshold > prevThreshold
                ? Mathf.Clamp01((float)(currentKills - prevThreshold) / (nextThreshold - prevThreshold))
                : 0f;

            float barHeight = 14f;
            float barWidth = Mathf.Min(118f, rect.width - 4f);
            float barY = rect.y + (rect.height - barHeight) / 2f;
            Rect inset = new Rect(rect.x + (rect.width - barWidth) / 2f, barY, barWidth, barHeight);

            Widgets.FillableBar(inset, fillPercent, BarFillTexture, BarBgTexture, doBorder: true);

            // Overlay label: "X / Y"
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
            Widgets.Label(inset, $"{currentKills} / {nextThreshold}");
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, $"{"Military_Rank_Progress_Tip".Translate(MilitaryRanks.TranslatedName(currentRank), MilitaryRanks.TranslatedName(nextRank), currentKills, nextThreshold)}");
        }

        public override int GetMinWidth(PawnTable table) => 130;
        public override int GetOptimalWidth(PawnTable table) => 130;

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
