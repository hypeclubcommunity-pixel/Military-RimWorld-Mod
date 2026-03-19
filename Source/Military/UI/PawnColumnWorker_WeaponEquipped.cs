using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_WeaponEquipped : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            ThingWithComps weapon = pawn.equipment?.Primary;
            string label = weapon != null ? weapon.LabelCap : "-";

            if (weapon != null)
            {
                float iconSize = 20f;
                float gap = 4f;
                Text.Font = GameFont.Small;
                float textWidth = Mathf.Min(Text.CalcSize(label).x + 4f, rect.width - iconSize - gap);
                float groupWidth = iconSize + gap + textWidth;
                float startX = rect.x + (rect.width - groupWidth) / 2f;

                Rect iconRect = new Rect(startX, rect.y + (rect.height - iconSize) / 2f, iconSize, iconSize);
                Widgets.ThingIcon(iconRect, weapon);

                Rect labelRect = new Rect(iconRect.xMax + gap, rect.y, textWidth, rect.height);
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = MilitaryTheme.TextPrimary;
                Widgets.Label(labelRect, label);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                TooltipHandler.TipRegion(rect, weapon.DescriptionFlavor ?? weapon.LabelCap);
            }
            else
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = MilitaryTheme.TextMuted;
                Widgets.Label(rect, "-");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        public override int GetMinWidth(PawnTable table) => 130;
        public override int GetOptimalWidth(PawnTable table) => 130;

        public override int Compare(Pawn a, Pawn b)
        {
            string nameA = a?.equipment?.Primary?.Label ?? "";
            string nameB = b?.equipment?.Primary?.Label ?? "";
            return string.Compare(nameA, nameB, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
