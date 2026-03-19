using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace Military
{
    public class QuestNode_Mission03 : QuestNode
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
                new Rule_String("questName", "Mission 3: Iron Verdict")
            });
            QuestGen.AddQuestDescriptionRules(new List<Rule>
            {
                new Rule_String("questDescription",
                    "Silas Vane's intelligence has revealed Helix Corporation's forward operating base in your sector. " +
                    "An arms dealer will arrive with equipment. Prepare your forces — Helix is coming in full strength. " +
                    "Eliminate the Helix Commander to break their grip on the sector.")
            });

            string initSignal    = quest.InitiateSignal;
            string successSignal = "PhantomStrike.Mission03.Success";
            string failSignal    = "PhantomStrike.Mission03.Fail";

            // ── Opening letter ─────────────────────────────────────────
            QuestPart_Letter openLetter = new QuestPart_Letter();
            openLetter.inSignal = initSignal;
            openLetter.letter   = LetterMaker.MakeLetter(
                "Military_Mission03_StartLabel".Translate(),
                "Military_Mission03_StartText".Translate(),
                LetterDefOf.PositiveEvent);
            quest.AddPart(openLetter);

            // ── Fail only if ALL colonists die ─────────────────────────
            QuestPart_FailOnAllColonistsDead deathWatch = new QuestPart_FailOnAllColonistsDead();
            deathWatch.outSignalFail = failSignal;
            deathWatch.pawns = new List<Pawn>(colonists);
            quest.AddPart(deathWatch);

            // ── SUCCESS: Helix Commander eliminated (fired by GameComponent)
            // Award silver
            QuestPart_AwardSilver silverReward = new QuestPart_AwardSilver();
            silverReward.inSignal = successSignal;
            silverReward.amount   = 2000;
            quest.AddPart(silverReward);

            // Award materials
            QuestPart_AwardResources materials = new QuestPart_AwardResources();
            materials.inSignal   = successSignal;
            materials.wood       = 0;
            materials.steel      = 500;
            materials.components = 30;
            materials.medicine   = 20;
            quest.AddPart(materials);

            // Award mission XP to all ranked pawns
            QuestPart_AwardMissionCount award = new QuestPart_AwardMissionCount();
            award.inSignal = successSignal;
            award.pawns    = new List<Pawn>(colonists);
            award.amount   = 15;
            quest.AddPart(award);

            // Success letter
            QuestPart_Letter successLetter = new QuestPart_Letter();
            successLetter.inSignal = successSignal;
            successLetter.letter   = LetterMaker.MakeLetter(
                "Military_Mission03_SuccessLabel".Translate(),
                "Military_Mission03_SuccessText".Translate(),
                LetterDefOf.PositiveEvent);
            quest.AddPart(successLetter);

            QuestPart_QuestEnd okEnd = new QuestPart_QuestEnd();
            okEnd.inSignal = successSignal;
            okEnd.outcome  = QuestEndOutcome.Success;
            quest.AddPart(okEnd);

            // ── FAIL: all colonists dead ────────────────────────────────
            QuestPart_Letter failLetter = new QuestPart_Letter();
            failLetter.inSignal = failSignal;
            failLetter.letter   = LetterMaker.MakeLetter(
                "Military_Mission03_FailLabel".Translate(),
                "Military_Mission03_FailText".Translate(),
                LetterDefOf.NegativeEvent);
            quest.AddPart(failLetter);

            QuestPart_QuestEnd failEnd = new QuestPart_QuestEnd();
            failEnd.inSignal = failSignal;
            failEnd.outcome  = QuestEndOutcome.Fail;
            quest.AddPart(failEnd);

            if (Prefs.DevMode)
                Log.Message("[Military] Mission 3 quest initialized — Iron Verdict");
        }

        protected override bool TestRunInt(Slate slate)
        {
            return Find.AnyPlayerHomeMap != null;
        }
    }
}
