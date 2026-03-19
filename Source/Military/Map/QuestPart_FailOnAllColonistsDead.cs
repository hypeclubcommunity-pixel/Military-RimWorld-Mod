using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Military
{
    /// <summary>
    /// Fires fail signal only when ALL tracked colonists are dead.
    /// Used by Mission 3 (fail = total wipe, not a single death).
    /// </summary>
    public class QuestPart_FailOnAllColonistsDead : QuestPart
    {
        public string outSignalFail;
        public List<Pawn> pawns = new List<Pawn>();

        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            base.Notify_PawnKilled(pawn, dinfo);
            if (pawns == null || !pawns.Contains(pawn))
                return;

            // Check if ALL tracked pawns are now dead
            bool allDead = pawns.All(p => p == null || p.Dead || p.Destroyed);
            if (allDead)
            {
                if (Prefs.DevMode)
                    Log.Message("[Military] All mission colonists are dead — firing fail signal");

                if (!string.IsNullOrEmpty(outSignalFail))
                    Find.SignalManager.SendSignal(new Signal(outSignalFail, false));
            }
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
