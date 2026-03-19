using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Military
{
    public class QuestPart_FailOnPawnDeath : QuestPart
    {
        public string outSignalFail;
        public List<Pawn> pawns = new List<Pawn>();

        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            base.Notify_PawnKilled(pawn, dinfo);
            if (pawns == null || !pawns.Contains(pawn))
                return;

            if (Prefs.DevMode)
                Log.Message($"[Military] Mission pawn killed: {pawn.LabelShort} — firing fail signal");

            if (!string.IsNullOrEmpty(outSignalFail))
                Find.SignalManager.SendSignal(new Signal(outSignalFail, false));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref outSignalFail, "outSignalFail");
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pawns == null)
                pawns = new List<Pawn>();
        }
    }
}
