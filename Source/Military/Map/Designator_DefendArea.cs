using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace Military
{
    public class Designator_DefendArea : Designator
    {
        public static Pawn TargetPawn;
        public static IntVec3? Corner1;
        private bool _finalized = false;

        public Designator_DefendArea()
        {
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (Map == null || !c.InBounds(Map))
                return false;

            if (c.Fogged(Map))
                return false;

            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (TargetPawn == null)
            {
                Find.DesignatorManager.Deselect();
                return;
            }

            if (Corner1 == null)
            {
                Corner1 = c;
                Messages.Message("Military_DefendAreaCorner1".Translate(), MessageTypeDefOf.SilentInput, false);
            }
            else
            {
                _finalized = true;
                CellRect rect = CellRect.FromLimits(Corner1.Value, c);
                List<IntVec3> cells = rect.Cells.ToList();

                MilitaryStatComp comp = MilitaryUtility.GetComp(TargetPawn);
                if (comp != null)
                {
                    MilitaryUtility.AssignDefendArea(TargetPawn, cells);
                    Messages.Message(
                        "Military_DefendAreaAssigned".Translate(TargetPawn.LabelShort),
                        TargetPawn, MessageTypeDefOf.PositiveEvent, false);
                }

                Corner1 = null;
                TargetPawn = null;
                Find.DesignatorManager.Deselect();
            }
        }

        public override void Deselected()
        {
            if (!_finalized)
            {
                Corner1 = null;
                TargetPawn = null;
            }
            _finalized = false;
        }

        public override void ProcessInput(Event ev)
        {
            if (TargetPawn == null)
            {
                Find.DesignatorManager.Deselect();
                return;
            }
            base.ProcessInput(ev);
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
