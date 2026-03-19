using RimWorld;
using Verse;

namespace Military
{
    public class QuestPart_AwardSilver : QuestPart
    {
        public string inSignal;
        public int amount;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag != inSignal)
                return;

            if (amount <= 0)
                return;

            // Find or create silver thing stack on player map
            Map map = Find.AnyPlayerHomeMap;
            if (map == null)
                return;

            Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            silver.stackCount = amount;

            // Try to drop near center of map
            if (!GenThing.TryDropAndSetForbidden(silver, map.Center, map, ThingPlaceMode.Near, out Thing result, false))
            {
                if (Prefs.DevMode)
                    Log.Warning("[Military] Failed to drop silver reward on map");
                return;
            }

            if (Prefs.DevMode)
                Log.Message($"[Military] Quest reward: +{amount} silver");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref amount, "amount", 0);
        }
    }
}
