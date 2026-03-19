using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class MainTabWindow_Military : MainTabWindow_PawnTable
    {
        protected override PawnTableDef PawnTableDef =>
            DefDatabase<PawnTableDef>.GetNamed("MilitaryTable");

        protected override IEnumerable<Pawn> Pawns =>
            PawnsFinder.AllMaps_FreeColonists
                .Where(p => MilitaryUtility.IsEligible(p));

        private const float HeaderHeight = 35f;

        public override void DoWindowContents(Rect inRect)
        {
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight);
            Widgets.DrawBoxSolid(headerRect, MilitaryTheme.PanelBackground);
            Widgets.DrawBoxSolid(new Rect(headerRect.x, headerRect.y, headerRect.width, 4f), MilitaryTheme.HeaderFill);
            Widgets.DrawBoxSolid(new Rect(headerRect.x, headerRect.yMax - 1f, headerRect.width, 1f), MilitaryTheme.PanelTrim);

            Rect iconRect = new Rect(headerRect.x + 8f, headerRect.y + 5f, 24f, 24f);
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, MilitaryTextures.MainButton);

            Text.Font = GameFont.Medium;
            GUI.color = MilitaryTheme.SectionTitle;
            Widgets.Label(new Rect(iconRect.xMax + 6f, headerRect.y + 3f, 220f, 28f), "Military");
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            Rect squadsButtonRect = new Rect(headerRect.xMax - 110f, headerRect.y + 3f, 100f, 28f);
            GUI.color = MilitaryTheme.Promote;
            if (Widgets.ButtonText(squadsButtonRect, "Squads"))
                Find.WindowStack.Add(new Window_SquadManager());
            GUI.color = Color.white;

            Rect tableRect = new Rect(inRect.x, inRect.y + HeaderHeight, inRect.width, inRect.height - HeaderHeight);
            base.DoWindowContents(tableRect);
        }
    }
}
