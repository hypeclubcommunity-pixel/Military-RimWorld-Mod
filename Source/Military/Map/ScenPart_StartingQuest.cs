using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace Military
{
    public class ScenPart_StartingQuest : ScenPart
    {
        public QuestScriptDef questScriptDef;

        public override void PostGameStart()
        {
            base.PostGameStart();
            if (questScriptDef == null)
                return;

            Slate slate = new Slate();
            Quest quest = QuestGen.Generate(questScriptDef, slate);
            if (quest == null)
                return;

            quest.SetInitiallyAccepted();
            Find.QuestManager.Add(quest);
            // SetInitiallyAccepted already fires InitiateSignal — do NOT send it again

            var manager = Current.Game?.GetComponent<GameComponent_MilitaryManager>();
            manager?.ScheduleMission1(Find.TickManager.TicksGame);

            if (Prefs.DevMode)
                Log.Message($"[Military] Starting quest generated: {quest.name} (state: {quest.State}, signal: {quest.InitiateSignal})");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref questScriptDef, "questScriptDef");
        }
    }
}
