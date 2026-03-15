using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class Designator_PatrolWaypoint : Designator
    {
        public static MilitaryStatComp TargetComp;
        public static Pawn TargetPawn;
        private bool _finalized = false;

        public Designator_PatrolWaypoint()
        {
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(Map))
                return false;
            
            if (c.Fogged(Map))
                return false;

            if (!c.Walkable(Map))
                return "Military_CantWalkHere".Translate();

            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (TargetComp == null || TargetPawn == null)
            {
                Find.DesignatorManager.Deselect();
                return;
            }

            TargetComp.patrolWaypoints.Add(c);
            int count = TargetComp.patrolWaypoints.Count;

            string msg = count < 4
                ? "Military_WaypointSet".Translate(count)
                : "Military_WaypointFinalizing".Translate(count);
            Messages.Message(msg, MessageTypeDefOf.SilentInput, false);

            if (count >= 4)
            {
                _finalized = true;
                FinalizePatrol();
                Find.DesignatorManager.Deselect();
            }
        }

        public override void Deselected()
        {
            if (!_finalized && TargetComp != null)
            {
                int count = TargetComp.patrolWaypoints.Count;
                if (count >= 2)
                {
                    FinalizePatrol();
                }
                else
                {
                    TargetComp.patrolWaypoints.Clear();
                    Messages.Message("Military_NeedAtLeast2Waypoints".Translate(), MessageTypeDefOf.RejectInput, false);
                    TargetPawn = null;
                    TargetComp = null;
                }
            }
            _finalized = false;
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            foreach (IntVec3 cell in cells)
            {
                if (TargetComp != null && TargetComp.patrolWaypoints.Count < 4)
                {
                    DesignateSingleCell(cell);
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            if (TargetComp == null || TargetPawn == null)
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

        private void FinalizePatrol()
        {
            if (TargetComp.patrolWaypoints.Count > 0)
            {
                TargetComp.isPatrolling = true;
                Job job = JobMaker.MakeJob(MilitaryJobDefOf.MilitaryPatrol);
                job.locomotionUrgency = LocomotionUrgency.Walk;
                TargetPawn.jobs.StartJob(job, JobCondition.InterruptForced);
                Messages.Message(
                    "Military_PatrolAssigned".Translate(TargetPawn.LabelShort),
                    TargetPawn, MessageTypeDefOf.PositiveEvent, false);
            }
            TargetPawn = null;
            TargetComp = null;
        }
    }
}
