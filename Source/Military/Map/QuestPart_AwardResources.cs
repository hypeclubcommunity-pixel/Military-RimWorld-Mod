using RimWorld;
using Verse;

namespace Military
{
    // Drops Wood, Steel, ComponentIndustrial, and Medicine on quest signal — Mission 1 reward
    public class QuestPart_AwardResources : QuestPart
    {
        public string inSignal;
        public int wood       = 300;
        public int steel      = 200;
        public int components = 10;
        public int medicine   = 10;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != inSignal) return;

            Map map = Find.AnyPlayerHomeMap;
            if (map == null) return;

            MilitaryUtility.DropStacks(map, ThingDefOf.WoodLog,               wood);
            MilitaryUtility.DropStacks(map, ThingDefOf.Steel,                  steel);
            MilitaryUtility.DropStacks(map, ThingDefOf.ComponentIndustrial,    components);
            MilitaryUtility.DropStacks(map, ThingDefOf.MedicineIndustrial,     medicine);

            if (Prefs.DevMode)
                Log.Message($"[Military] M1 supply cache dropped: {wood} wood, {steel} steel, {components} components, {medicine} medicine");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal,   "inSignal");
            Scribe_Values.Look(ref wood,       "wood",       300);
            Scribe_Values.Look(ref steel,      "steel",      200);
            Scribe_Values.Look(ref components, "components", 10);
            Scribe_Values.Look(ref medicine,   "medicine",   10);
        }
    }
}
