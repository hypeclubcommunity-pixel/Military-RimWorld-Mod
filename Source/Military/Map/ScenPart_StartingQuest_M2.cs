using RimWorld;
using Verse;

namespace Military
{
    // Placeholder ScenPart — Mission 2 is triggered by GameComponent_MilitaryManager
    // after Mission 1 completes. This exists so the scenario XML can reference it.
    public class ScenPart_StartingQuest_M2 : ScenPart
    {
        public QuestScriptDef questScriptDef;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref questScriptDef, "questScriptDef");
        }
    }
}
