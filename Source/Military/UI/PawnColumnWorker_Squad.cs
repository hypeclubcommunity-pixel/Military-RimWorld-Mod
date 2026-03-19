using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class PawnColumnWorker_Squad : PawnColumnWorker
    {
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.squadId))
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = MilitaryTheme.TextMuted;
                Widgets.Label(rect, "-");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            SquadData squad = GameComponent_MilitaryManager.Instance?.GetSquadById(comp.squadId);
            if (squad == null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = MilitaryTheme.TextMuted;
                Widgets.Label(rect, "-");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            string role = comp.isSquadLeader ? "Leader" : "Member";
            string label = $"[{squad.squadName}] {role}";

            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = comp.isSquadLeader ? MilitaryTheme.SectionTitle : MilitaryTheme.TextPrimary;
            Widgets.Label(rect, label);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override int GetMinWidth(PawnTable table) => 120;
        public override int GetOptimalWidth(PawnTable table) => 120;

        public override int Compare(Pawn a, Pawn b)
        {
            string squadA = MilitaryUtility.GetComp(a)?.squadId ?? "";
            string squadB = MilitaryUtility.GetComp(b)?.squadId ?? "";
            return string.Compare(squadA, squadB, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
