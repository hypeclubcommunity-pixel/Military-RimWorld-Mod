using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobGiver_AutoCapture : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Downed || pawn.Dead) return null;
            if (pawn.Map == null) return null;

            var comp = pawn.TryGetComp<MilitaryStatComp>();
            if (comp == null || string.IsNullOrEmpty(comp.rank)) return null;

            if (!pawn.health.capacities.CanBeAwake) return null;
            if (pawn.IsBurning()) return null;

            Building_Bed prisonBed = RestUtility.FindBedFor(
                pawn, pawn, false, false, GuestStatus.Prisoner);
            if (prisonBed == null) return null;

            Pawn target = null;
            float closest = float.MaxValue;

            foreach (Pawn p in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!p.Downed) continue;
                if (p.Dead) continue;
                if (p.Faction == null) continue;
                if (!p.Faction.HostileTo(Faction.OfPlayer)) continue;
                if (!p.RaceProps.Humanlike) continue;
                if (!pawn.CanReserveAndReach(p, PathEndMode.Touch,
                    Danger.Deadly)) continue;

                Building_Bed bed = RestUtility.FindBedFor(
                    p, pawn, true, false, GuestStatus.Prisoner);
                if (bed == null) continue;

                float dist = pawn.Position.DistanceTo(p.Position);
                if (dist < closest)
                {
                    closest = dist;
                    target = p;
                }
            }

            if (target == null) return null;

            Building_Bed targetBed = RestUtility.FindBedFor(
                target, pawn, true, false, GuestStatus.Prisoner);
            if (targetBed == null) return null;

            Job job = JobMaker.MakeJob(JobDefOf.Capture, target, targetBed);
            job.count = 1;
            return job;
        }
    }
}
