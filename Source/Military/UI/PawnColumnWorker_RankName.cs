using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_RankName : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            bool hasRank = !string.IsNullOrEmpty(comp.rank);
            string displayText = hasRank ? MilitaryRanks.TranslatedName(comp.rank) : "-";

            if (hasRank && comp.IsPromotionAvailable)
                displayText += " ↑";

            if (Widgets.ButtonInvisible(rect))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                if (!hasRank)
                {
                    string recruitRank = MilitaryRanks.Default;
                    options.Add(new FloatMenuOption(MilitaryRanks.TranslatedName(recruitRank), () =>
                    {
                        MilitaryUtility.SetRank(pawn, recruitRank);
                    }));
                }
                else
                {
                    string currentRank = comp.rank;
                    string nextRank = MilitaryRanks.Next(currentRank);
                    bool isMaxRank = currentRank == MilitaryRanks.All[MilitaryRanks.All.Count - 1];
                    bool canPromote = !isMaxRank && MilitaryRanks.IsEligibleForRank(nextRank, comp.missionCount);

                    if (isMaxRank)
                    {
                        options.Add(new FloatMenuOption("Military_DisabledMaxRank".Translate(), null));
                    }
                    else if (canPromote)
                    {
                        options.Add(new FloatMenuOption(MilitaryRanks.TranslatedName(nextRank), () =>
                        {
                            MilitaryUtility.SetRank(pawn, nextRank);
                            MilitaryUtility.SendPromotionLetter(pawn, nextRank);
                        }));
                    }
                    else
                    {
                        options.Add(new FloatMenuOption("Military_RequiresKills".Translate(MilitaryRanks.KillThresholds[nextRank]), null));
                    }
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            if (hasRank)
            {
                float iconSize = 20f;
                float gap = 4f;
                float textWidth = Mathf.Min(Text.CalcSize(displayText).x + 4f, rect.width - iconSize - gap);
                float groupWidth = iconSize + gap + textWidth;
                float startX = rect.x + (rect.width - groupWidth) / 2f;

                Rect iconRect = new Rect(startX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                Widgets.DrawTextureFitted(iconRect, MilitaryUtility.GetRankTexture(comp.rank), 1f);

                Rect textRect = new Rect(iconRect.xMax + gap, rect.y, textWidth, rect.height);
                Widgets.Label(textRect, displayText);
            }
            else
            {
                Widgets.Label(rect, displayText);
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override int GetMinWidth(PawnTable table) => 110;
        public override int GetOptimalWidth(PawnTable table) => 110;

        public override int Compare(Pawn a, Pawn b)
        {
            MilitaryStatComp compA = MilitaryUtility.GetComp(a);
            MilitaryStatComp compB = MilitaryUtility.GetComp(b);
            int indexA = compA != null && !string.IsNullOrEmpty(compA.rank) ? MilitaryRanks.All.IndexOf(compA.rank) : -1;
            int indexB = compB != null && !string.IsNullOrEmpty(compB.rank) ? MilitaryRanks.All.IndexOf(compB.rank) : -1;
            return indexA.CompareTo(indexB);
        }
    }
}
