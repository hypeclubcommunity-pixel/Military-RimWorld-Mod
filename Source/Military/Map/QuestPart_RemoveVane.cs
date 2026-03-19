using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class QuestPart_RemoveVane : QuestPart
    {
        public string inSignal;
        public Pawn vanePawn;
        public string outSignalChain;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag != inSignal)
                return;

            // Fire chain signal immediately so Mission 3 can begin
            if (!string.IsNullOrEmpty(outSignalChain))
                Find.SignalManager.SendSignal(new Signal(outSignalChain, true));

            if (vanePawn == null || vanePawn.Dead)
            {
                if (Prefs.DevMode)
                    Log.Message("[Military] Silas Vane already gone at farewell signal");
                return;
            }

            if (vanePawn.Spawned && vanePawn.Map != null)
            {
                // Give Vane a job to walk to the map edge and exit naturally
                Map map = vanePawn.Map;
                IntVec3 exitCell;
                if (!CellFinder.TryFindRandomEdgeCellWith(
                        c => c.Standable(map), map, CellFinder.EdgeRoadChance_Ignore, out exitCell))
                    exitCell = CellFinder.RandomEdgeCell(map);

                Job exitJob = JobMaker.MakeJob(JobDefOf.Goto, exitCell);
                exitJob.exitMapOnArrival = true;
                vanePawn.jobs.StartJob(exitJob, JobCondition.InterruptForced);

                if (Prefs.DevMode)
                    Log.Message($"[Military] Silas Vane walking off map to {exitCell}");
            }
            else
            {
                vanePawn.DeSpawn();
                if (Prefs.DevMode)
                    Log.Message("[Military] Silas Vane removed (not on map)");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_References.Look(ref vanePawn, "vanePawn");
            Scribe_Values.Look(ref outSignalChain, "outSignalChain");
        }
    }
}
