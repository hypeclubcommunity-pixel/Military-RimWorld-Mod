using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobDriver_Patrol : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);

            // If no comp or no waypoints, end immediately
            if (comp == null || comp.patrolWaypoints == null || comp.patrolWaypoints.Count == 0)
            {
                Toil endToil = new Toil();
                endToil.initAction = () =>
                {
                    EndJobWith(JobCondition.Succeeded);
                };
                yield return endToil;
                yield break;
            }

            // Create a toil that loops through waypoints indefinitely
            Toil patrolToil = new Toil();
            int waypointIndex = 0;

            patrolToil.initAction = () =>
            {
                waypointIndex = 0;
            };

            patrolToil.tickAction = () =>
            {
                // Check if patrolling was cancelled
                if (!comp.isPatrolling)
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }

                // Danger awareness: every 120 ticks, scan for nearby hostiles
                if (pawn.IsHashIntervalTick(120))
                {
                    IReadOnlyList<Pawn> allPawns = pawn.Map.mapPawns.AllPawnsSpawned;
                    for (int i = 0; i < allPawns.Count; i++)
                    {
                        Pawn hostile = allPawns[i];
                        if (hostile.Faction != null
                            && hostile.Faction.HostileTo(pawn.Faction)
                            && hostile.Spawned
                            && !hostile.Downed
                            && pawn.Position.DistanceTo(hostile.Position) <= 20f)
                        {
                            // End patrol job — auto-attack ThinkTree will handle engagement,
                            // watchdog will resume patrol after combat ends.
                            Messages.Message(
                                "Military_PatrolBroken".Translate(pawn.LabelShort),
                                pawn, MessageTypeDefOf.CautionInput, false);
                            EndJobWith(JobCondition.InterruptForced);
                            return;
                        }
                    }
                }

                // If pawn has reached the current waypoint or is not moving, go to next
                if (pawn.pather == null || !pawn.pather.Moving)
                {
                    IntVec3 target = comp.patrolWaypoints[waypointIndex];
                    pawn.pather.StartPath(target, PathEndMode.OnCell);

                    waypointIndex++;
                    if (waypointIndex >= comp.patrolWaypoints.Count)
                        waypointIndex = 0;
                }
            };

            patrolToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return patrolToil;
        }
    }
}
