using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace Military
{
    public class QuestNode_Mission01 : QuestNode
    {
        protected override void RunInt()
        {
            Quest quest = QuestGen.quest;
            Map map = Find.AnyPlayerHomeMap;
            if (map == null)
                return;

            List<Pawn> colonists = new List<Pawn>(map.mapPawns.FreeColonists);

            QuestGen.AddQuestNameRules(new List<Rule>
            {
                new Rule_String("questName", "Mission 1: No Safe Ground")
            });
            QuestGen.AddQuestDescriptionRules(new List<Rule>
            {
                new Rule_String("questDescription", "You escaped from Helix Corporation. They tracked your drop pods and an eraser team is already converging on your position. Eliminate every last one of them. Lose one operator and it is over.")
            });

            string initSignal    = quest.InitiateSignal;
            string successSignal = "PhantomStrike.Mission01.Success";
            string failSignal    = "PhantomStrike.Mission01.Fail";

            // ── Opening story letter ─────────────────────────────────────
            QuestPart_Letter openLetter = new QuestPart_Letter();
            openLetter.inSignal = initSignal;
            openLetter.letter   = LetterMaker.MakeLetter(
                "Military_Mission01_StartLabel".Translate(),
                "Military_Mission01_StartText".Translate(),
                LetterDefOf.ThreatBig);
            quest.AddPart(openLetter);

            // ── Fail if any starting pawn dies ───────────────────────────
            QuestPart_FailOnPawnDeath deathWatch = new QuestPart_FailOnPawnDeath();
            deathWatch.outSignalFail = failSignal;
            deathWatch.pawns = new List<Pawn>(colonists);
            quest.AddPart(deathWatch);

            // ── SUCCESS: all Helix raiders defeated (fired by GameComponent)
            // Drop building materials first
            QuestPart_AwardResources materials = new QuestPart_AwardResources();
            materials.inSignal  = successSignal;
            materials.wood       = 300;
            materials.steel      = 200;
            materials.components = 10;
            materials.medicine   = 10;
            quest.AddPart(materials);

            QuestPart_Letter successLetter = new QuestPart_Letter();
            successLetter.inSignal = successSignal;
            successLetter.letter   = LetterMaker.MakeLetter(
                "Military_Mission01_SuccessLabel".Translate(),
                "Military_Mission01_SuccessText".Translate(),
                LetterDefOf.PositiveEvent);
            quest.AddPart(successLetter);

            QuestPart_AwardMissionCount award = new QuestPart_AwardMissionCount();
            award.inSignal = successSignal;
            award.pawns    = new List<Pawn>(colonists);
            award.amount   = 8;
            quest.AddPart(award);

            QuestPart_QuestEnd okEnd = new QuestPart_QuestEnd();
            okEnd.inSignal = successSignal;
            okEnd.outcome  = QuestEndOutcome.Success;
            quest.AddPart(okEnd);

            // ── FAIL: pawn died ──────────────────────────────────────────
            QuestPart_Letter failLetter = new QuestPart_Letter();
            failLetter.inSignal = failSignal;
            failLetter.letter   = LetterMaker.MakeLetter(
                "Military_Mission01_FailLabel".Translate(),
                "Military_Mission01_FailText".Translate(),
                LetterDefOf.NegativeEvent);
            quest.AddPart(failLetter);

            QuestPart_QuestEnd failEnd = new QuestPart_QuestEnd();
            failEnd.inSignal = failSignal;
            failEnd.outcome  = QuestEndOutcome.Fail;
            quest.AddPart(failEnd);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return Find.AnyPlayerHomeMap != null;
        }
    }
}
