using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Military
{
    public class QuestPart_AwardMissionCount : QuestPart
    {
        public string inSignal;
        public List<Pawn> pawns = new List<Pawn>();
        public int amount;
        public string outSignalChain;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag != inSignal)
                return;

            if (pawns != null)
            {
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn pawn = pawns[i];
                    if (pawn == null || pawn.Dead)
                        continue;

                    MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
                    if (comp == null)
                        continue;

                    comp.missionCount += amount;

                    if (Prefs.DevMode)
                        Log.Message($"[Military] Quest: +{amount} kills → {pawn.LabelShort} (total: {comp.missionCount})");
                }
            }

            if (!string.IsNullOrEmpty(outSignalChain))
                Find.SignalManager.SendSignal(new Signal(outSignalChain, true));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref amount, "amount");
            Scribe_Values.Look(ref outSignalChain, "outSignalChain");
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && pawns == null)
                pawns = new List<Pawn>();
        }
    }
}
