using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class MilitaryStatComp : ThingComp
    {
        public string rank = "";
        public int missionCount = 0;
        public List<IntVec3> patrolWaypoints = new List<IntVec3>();
        public bool isPatrolling = false;

        public bool IsPromotionAvailable =>
            !string.IsNullOrEmpty(rank) && MilitaryRanks.NextEligibleRank(rank, missionCount) != null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref rank, "rank", "");
            Scribe_Values.Look(ref missionCount, "missionCount", 0);
            Scribe_Collections.Look(ref patrolWaypoints, "patrolWaypoints", LookMode.Value);
            Scribe_Values.Look(ref isPatrolling, "isPatrolling", false);
            if (patrolWaypoints == null)
                patrolWaypoints = new List<IntVec3>();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad && parent is Pawn pawn)
            {
                if (!MilitaryUtility.IsEligible(pawn))
                {
                    if (isPatrolling)
                    {
                        isPatrolling = false;
                        pawn.jobs?.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
                    }
                    rank = "";
                    patrolWaypoints.Clear();
                }
            }
        }

        // Jobs that indicate a patrolling pawn drifted and should be nudged back.
        private static readonly HashSet<string> PatrolIdleJobs = new HashSet<string>
        {
            "Wait",
            "Wait_Wander",
            "GotoWander",
            "Clean",
        };

        // Jobs that indicate combat is over and patrol can safely resume.
        // Excludes Wait_Combat so we don't interrupt an active fight.
        private static readonly HashSet<string> ResumeIdleJobs = new HashSet<string>
        {
            "Wait",
            "Wait_Wander",
            "GotoWander",
            "Clean",
        };

        public override void CompTick()
        {
            base.CompTick();

            if (parent is not Pawn pawn)
                return;

            // Fast flee cancel — ranked colonists don't flee.
            if (pawn.IsHashIntervalTick(30)
                && pawn.IsFreeColonist
                && !string.IsNullOrEmpty(rank)
                && (pawn.CurJob?.def == JobDefOf.Flee || pawn.CurJob?.def == JobDefOf.FleeAndCower))
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                return;
            }

            if (!pawn.IsHashIntervalTick(200))
                return;

            if (patrolWaypoints.Count < 2)
                return;

            if (!pawn.IsFreeColonist || pawn.Drafted)
                return;

            JobDef curDef = pawn.CurJob?.def;

            if (isPatrolling)
            {
                // Patrolling but drifted to an idle job — restart patrol.
                if (curDef != null && !PatrolIdleJobs.Contains(curDef.defName))
                    return;
            }
            else
            {
                // Not patrolling but waypoints preserved — resume after combat
                // only when the pawn is truly idle (not mid-fight).
                if (curDef != null && !ResumeIdleJobs.Contains(curDef.defName))
                    return;
                isPatrolling = true;
            }

            Job job = JobMaker.MakeJob(MilitaryJobDefOf.MilitaryPatrol);
            job.locomotionUrgency = LocomotionUrgency.Walk;
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }

        public override string CompInspectStringExtra()
        {
            if (parent is Pawn pawn && pawn.IsColonist)
            {
                string rankLine = "Military_InspectRank".Translate(MilitaryRanks.TranslatedName(rank));
                string missionLine = "Military_InspectMissions".Translate(missionCount);
                return $"{rankLine}\n{missionLine}";
            }
            return null;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (parent is Pawn pawn && pawn.IsColonist)
            {
                foreach (var gizmo in RankGizmo.GetGizmos(pawn))
                {
                    yield return gizmo;
                }
                foreach (var gizmo in RankGizmo.GetPatrolGizmos(pawn))
                {
                    yield return gizmo;
                }
            }
        }
    }

    public class CompProperties_MilitaryStat : CompProperties
    {
        public CompProperties_MilitaryStat()
        {
            compClass = typeof(MilitaryStatComp);
        }
    }
}
