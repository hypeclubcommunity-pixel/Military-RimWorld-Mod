using System.Collections.Generic;
using System.Linq;
using RimWorld;
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
    }
}
