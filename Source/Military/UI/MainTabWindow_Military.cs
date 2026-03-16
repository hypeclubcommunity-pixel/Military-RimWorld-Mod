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
            Rect squadsButtonRect = new Rect(headerRect.xMax - 110f, headerRect.y + 3f, 100f, 28f);
            if (Widgets.ButtonText(squadsButtonRect, "Squads"))
                Find.WindowStack.Add(new Window_SquadManager());

            Rect tableRect = new Rect(inRect.x, inRect.y + HeaderHeight, inRect.width, inRect.height - HeaderHeight);
            base.DoWindowContents(tableRect);
        }
    }
}
