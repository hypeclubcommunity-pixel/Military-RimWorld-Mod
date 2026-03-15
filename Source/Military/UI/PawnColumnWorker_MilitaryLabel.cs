using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_MilitaryLabel : PawnColumnWorker_Label
    {
        private const float StripWidth = 4f;
        private const float PortraitSize = 40f;
        private const float PortraitGap = 4f;

        private static readonly Color RankRecruit = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color RankPrivate = new Color(0.2f, 0.6f, 0.2f);
        private static readonly Color RankCorporal = new Color(0.2f, 0.4f, 0.8f);
        private static readonly Color RankSergeant = new Color(0.8f, 0.5f, 0.1f);
        private static readonly Color RankLieutenant = new Color(0.9f, 0.7f, 0.1f);

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            bool hasRank = comp != null && !string.IsNullOrEmpty(comp.rank);

            // Rank color strip on left edge
            if (hasRank)
            {
                Color color = GetRankColor(comp.rank);
                Rect stripRect = new Rect(rect.x, rect.y, StripWidth, rect.height);
                Widgets.DrawBoxSolid(stripRect, color);
            }

            string displayName = pawn.LabelShortCap;
            Text.Font = GameFont.Small;
            float availableWidth = rect.width - StripWidth;
            float textWidth = Mathf.Min(Text.CalcSize(displayName).x + 6f, availableWidth);

            Rect labelRect;
            if (pawn.RaceProps.Humanlike)
            {
                float contentWidth = PortraitSize + PortraitGap + textWidth;
                float contentStartX = rect.x + StripWidth + ((availableWidth - contentWidth) / 2f);

                Rect portraitRect = new Rect(contentStartX, rect.y + (rect.height - PortraitSize) / 2f, PortraitSize, PortraitSize);
                RenderTexture portrait = PortraitsCache.Get(
                    pawn,
                    new Vector2(PortraitSize, PortraitSize),
                    Rot4.South,
                    Vector3.zero,
                    1f,
                    supersample: true,
                    compensateForUIScale: true,
                    renderHeadgear: true,
                    renderClothes: true,
                    overrideApparelColors: null,
                    overrideHairColor: null,
                    stylingStation: false,
                    healthStateOverride: null
                );
                GUI.DrawTexture(portraitRect, portrait);

                labelRect = new Rect(portraitRect.xMax + PortraitGap, rect.y, textWidth, rect.height);
            }
            else
            {
                labelRect = new Rect(rect.x + StripWidth, rect.y, rect.width - StripWidth, rect.height);
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(labelRect, displayName);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private Color GetRankColor(string rank)
        {
            return rank switch
            {
                "Recruit" => RankRecruit,
                "Private" => RankPrivate,
                "Corporal" => RankCorporal,
                "Sergeant" => RankSergeant,
                "Lieutenant" => RankLieutenant,
                _ => Color.gray
            };
        }

        public override int GetMinWidth(PawnTable table) => 180;
        public override int GetOptimalWidth(PawnTable table) => 180;
    }
}
