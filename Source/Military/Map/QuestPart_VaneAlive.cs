using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Military
{
    public class QuestPart_VaneAlive : QuestPart
    {
        public string outSignalFail;
        public Pawn vanePawn;

        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            base.Notify_PawnKilled(pawn, dinfo);
            if (pawn != vanePawn)
                return;

            if (Prefs.DevMode)
                Log.Message($"[Military] Silas Vane killed — firing fail signal");

            if (!string.IsNullOrEmpty(outSignalFail))
                Find.SignalManager.SendSignal(new Signal(outSignalFail, false));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref outSignalFail, "outSignalFail");
            Scribe_References.Look(ref vanePawn, "vanePawn");
        }
    }
}
