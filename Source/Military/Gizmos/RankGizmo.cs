using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    [DefOf]
    public static class MilitaryJobDefOf
    {
        public static JobDef MilitaryPatrol;
    }

    public static class RankGizmo
    {
        public static IEnumerable<Gizmo> GetGizmos(Pawn pawn)
        {
            if (pawn == null || !MilitaryUtility.IsEligible(pawn))
                yield break;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                yield break;

            if (string.IsNullOrEmpty(comp.rank))
                yield break;

            string currentRank = comp.rank;
            string nextRank = MilitaryRanks.Next(currentRank);
            string prevRank = MilitaryRanks.Previous(currentRank);
            bool isMaxRank = currentRank == MilitaryRanks.All[MilitaryRanks.All.Count - 1];
            bool isMinRank = currentRank == MilitaryRanks.All[0];

            // Promote button
            var promote = new Command_Action
            {
                icon = MilitaryTextures.Promote,
                defaultLabel = "Military_Gizmo_Promote".Translate(),
                defaultDesc = "Military_Gizmo_PromoteDesc".Translate(
                    MilitaryRanks.TranslatedName(currentRank),
                    MilitaryRanks.TranslatedName(nextRank)),
                action = () =>
                {
                    MilitaryUtility.SetRank(pawn, nextRank);
                    MilitaryUtility.SendPromotionLetter(pawn, nextRank);
                }
            };
            if (isMaxRank)
            {
                promote.Disable("Military_DisabledMaxRank".Translate());
            }
            else if (!MilitaryRanks.IsEligibleForRank(nextRank, comp.missionCount))
            {
                promote.Disable("Military_RequiresKills".Translate(MilitaryRanks.KillThresholds[nextRank]));
            }
            yield return promote;

            // Demote button
            var demote = new Command_Action
            {
                icon = MilitaryTextures.Demote,
                defaultLabel = "Military_Gizmo_Demote".Translate(),
                defaultDesc = "Military_Gizmo_DemoteDesc".Translate(
                    MilitaryRanks.TranslatedName(currentRank),
                    MilitaryRanks.TranslatedName(prevRank)),
                action = () =>
                {
                    MilitaryUtility.SetRank(pawn, prevRank);
                }
            };
            if (isMinRank)
            {
                demote.Disable("Military_DisabledMinRank".Translate());
            }
            yield return demote;
        }

        public static IEnumerable<Gizmo> GetPatrolGizmos(Pawn pawn)
        {
            if (pawn == null || !MilitaryUtility.IsEligible(pawn))
                yield break;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                yield break;

            // "Stop Patrol" button — shown when this pawn is currently patrolling
            if (comp.isPatrolling)
            {
                var stopPatrol = new Command_Action
                {
                    icon = MilitaryTextures.StopPatrol,
                    defaultLabel = "Military_Gizmo_StopPatrol".Translate(),
                    defaultDesc = "Military_Gizmo_StopPatrolDesc".Translate(),
                    action = () =>
                    {
                        comp.isPatrolling = false;
                        comp.patrolWaypoints.Clear();
                        if (pawn.jobs?.curJob?.def == MilitaryJobDefOf.MilitaryPatrol)
                        {
                            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        }
                    }
                };
                yield return stopPatrol;
            }

            // "Assign Patrol" button — only for Lieutenants
            if (comp.rank == "Lieutenant")
            {
                var assignPatrol = new Command_Action
                {
                    icon = MilitaryTextures.AssignPatrol,
                    defaultLabel = "Military_Gizmo_AssignPatrol".Translate(),
                    defaultDesc = "Military_Gizmo_AssignPatrolDesc".Translate(),
                    action = () =>
                    {
                        // Step 1: Target a colonist on the map
                        Find.Targeter.BeginTargeting(
                            new TargetingParameters
                            {
                                canTargetPawns = true,
                                canTargetBuildings = false,
                                canTargetLocations = false,
                                canTargetItems = false,
                                validator = (TargetInfo info) =>
                                {
                                    if (info.Thing is not Pawn candidate)
                                        return false;
                                    if (!candidate.IsColonist)
                                        return false;
                                    if (!MilitaryUtility.IsEligible(candidate))
                                        return false;
                                    MilitaryStatComp targetComp = MilitaryUtility.GetComp(candidate);
                                    if (targetComp == null)
                                        return false;
                                    if (targetComp.isPatrolling)
                                        return false;
                                    string rank = targetComp.rank;
                                    return rank == "Recruit" || rank == "Private" || rank == "Corporal" || rank == "";
                                }
                            },
                            (LocalTargetInfo targetInfo) =>
                            {
                                // Colonist selected — activate the custom Designator waypoint assigner
                                if (targetInfo.Thing is Pawn selectedPawn)
                                {
                                    MilitaryStatComp targetComp = MilitaryUtility.GetComp(selectedPawn);
                                    if (targetComp != null)
                                    {
                                        targetComp.patrolWaypoints.Clear();
                                        Designator_PatrolWaypoint.TargetPawn = selectedPawn;
                                        Designator_PatrolWaypoint.TargetComp = targetComp;
                                        
                                        Find.DesignatorManager.Select(new Designator_PatrolWaypoint());
                                        Messages.Message("Military_WaypointBegin".Translate(), MessageTypeDefOf.SilentInput, false);
                                    }
                                }
                            },
                            pawn,
                            null,
                            null
                        );
                    }
                };
                yield return assignPatrol;
            }
        }

        public static IEnumerable<Gizmo> GetBodyguardGizmos(Pawn pawn)
        {
            if (pawn == null || !MilitaryUtility.IsEligible(pawn))
                yield break;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                yield break;

            if (comp.rank == "Lieutenant")
                yield break;

            int rankIndex = MilitaryRanks.All.IndexOf(comp.rank);
            int sergeantIndex = MilitaryRanks.All.IndexOf("Sergeant");
            if (rankIndex < 0 || rankIndex > sergeantIndex)
                yield break;

            if (comp.bodyguardTarget == null)
            {
                var assign = new Command_Action
                {
                    icon = MilitaryTextures.AssignBodyguard,
                    defaultLabel = "Military_AssignBodyguard".Translate(),
                    defaultDesc = "Military_AssignBodyguardDesc".Translate(),
                    action = () =>
                    {
                        Find.Targeter.BeginTargeting(
                            new TargetingParameters
                            {
                                canTargetPawns = true,
                                canTargetBuildings = false,
                                canTargetLocations = false,
                                canTargetItems = false,
                                validator = (TargetInfo info) =>
                                    info.Thing is Pawn target
                                    && target.Spawned
                                    && target != pawn
                                    && target.Faction == Faction.OfPlayer
                            },
                            (LocalTargetInfo targetInfo) =>
                            {
                                if (targetInfo.Thing is Pawn target)
                                {
                                    if (MilitaryUtility.AssignBodyguard(pawn, target))
                                    {
                                        Messages.Message("Military_BodyguardAssigned".Translate(pawn.LabelShort, target.LabelShort),
                                            MessageTypeDefOf.PositiveEvent, false);
                                    }
                                }
                            },
                            pawn,
                            null,
                            null
                        );
                    }
                };
                yield return assign;
            }
            else
            {
                var stop = new Command_Action
                {
                    icon = MilitaryTextures.StopBodyguard,
                    defaultLabel = "Military_StopBodyguard".Translate(),
                    defaultDesc = "Military_StopBodyguardDesc".Translate(),
                    action = () =>
                    {
                        MilitaryUtility.ClearBodyguard(pawn);
                        Messages.Message("Military_BodyguardCleared".Translate(pawn.LabelShort),
                            MessageTypeDefOf.NeutralEvent, false);
                    }
                };
                yield return stop;
            }
        }

        public static IEnumerable<Gizmo> GetDefendAreaGizmos(Pawn pawn)
        {
            if (pawn == null || !MilitaryUtility.IsEligible(pawn))
                yield break;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                yield break;

            if (comp.rank == "Lieutenant")
            {
                var assignDefend = new Command_Action
                {
                    icon = MilitaryTextures.AssignDefend,
                    defaultLabel = "Military_AssignDefendArea".Translate(),
                    defaultDesc = "Military_AssignDefendAreaDesc".Translate(),
                    action = () =>
                    {
                        Find.Targeter.BeginTargeting(
                            new TargetingParameters
                            {
                                canTargetPawns = true,
                                canTargetBuildings = false,
                                canTargetLocations = false,
                                canTargetItems = false,
                                validator = (TargetInfo info) =>
                                {
                                    if (info.Thing is not Pawn candidate)
                                        return false;
                                    if (!candidate.IsColonist || !MilitaryUtility.IsEligible(candidate))
                                        return false;
                                    MilitaryStatComp targetComp = MilitaryUtility.GetComp(candidate);
                                    return targetComp != null && !string.IsNullOrEmpty(targetComp.rank);
                                }
                            },
                            (LocalTargetInfo targetInfo) =>
                            {
                                if (targetInfo.Thing is Pawn selectedPawn)
                                {
                                    Designator_DefendArea.TargetPawn = selectedPawn;
                                    Designator_DefendArea.Corner1 = null;
                                    Find.DesignatorManager.Select(new Designator_DefendArea());
                                    Messages.Message("Military_DefendAreaBegin".Translate(), MessageTypeDefOf.SilentInput, false);
                                }
                            },
                            pawn,
                            null,
                            null
                        );
                    }
                };
                yield return assignDefend;
            }

            if (comp.isDefending)
            {
                var stopDefend = new Command_Action
                {
                    icon = MilitaryTextures.StopDefend,
                    defaultLabel = "Military_StopDefending".Translate(),
                    defaultDesc = "Military_StopDefendingDesc".Translate(),
                    action = () =>
                    {
                        MilitaryUtility.ClearDefendArea(pawn);
                    }
                };
                yield return stopDefend;
            }
        }
    }
}
