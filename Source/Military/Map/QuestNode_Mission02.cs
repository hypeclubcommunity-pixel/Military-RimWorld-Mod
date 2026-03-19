using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;
using Verse.AI;

namespace Military
{
    public class QuestNode_Mission02 : QuestNode
    {
        protected override void RunInt()
        {
            Quest quest = QuestGen.quest;
            Map map = Find.AnyPlayerHomeMap;
            if (map == null)
                return;

            // ── Grammar rules ─────────────────────────────────────
            QuestGen.AddQuestNameRules(new List<Rule>
            {
                new Rule_String("questName", "Mission 2: Vanguard's Shadow")
            });
            QuestGen.AddQuestDescriptionRules(new List<Rule>
            {
                new Rule_String("questDescription", "Silas Vane is a defecting intelligence officer carrying critical data that can hurt Helix Corporation. Command is routing his extraction through your position. Eraser teams are hunting him. Keep Vane alive on your map for 7 days until extraction. Assign him a bodyguard if possible.")
            });

            string initSignal = quest.InitiateSignal;
            string timerCompleteSignal = "PhantomStrike.Mission02.TimerComplete";
            string failSignal = "PhantomStrike.Mission02.Fail";
            string chainSignal = "PhantomStrike.Mission03.Start";

            // ── Spawn Silas Vane at home map ──────────────────────
            Pawn vane = SpawnVane(map);
            if (vane == null)
                return;

            // ── Opening letter ────────────────────────────────────
            QuestPart_Letter openLetter = new QuestPart_Letter();
            openLetter.inSignal = initSignal;
            openLetter.letter = LetterMaker.MakeLetter(
                "Military_Mission02_StartLabel".Translate(),
                "Military_Mission02_StartText".Translate(),
                LetterDefOf.ThreatBig,
                new LookTargets(vane));
            quest.AddPart(openLetter);

            // ── 7-day timer (168,000 ticks) ──────────────────────
            QuestPart_Delay timer = new QuestPart_Delay();
            timer.inSignalEnable = quest.InitiateSignal;
            timer.delayTicks = 168000;
            timer.expiryInfoPart = "Military_Mission02_Title".Translate();
            timer.expiryInfoPartTip = "Military_Mission02_ProtectText".Translate();
            timer.outSignalsCompleted = new List<string> { timerCompleteSignal };
            quest.AddPart(timer);

            // ── Fail if Vane dies ─────────────────────────────────
            QuestPart_VaneAlive vaneWatch = new QuestPart_VaneAlive();
            vaneWatch.outSignalFail = failSignal;
            vaneWatch.vanePawn = vane;
            quest.AddPart(vaneWatch);

            // ── SUCCESS: timer expired, Vane alive ────────────────
            QuestPart_Letter successLetter = new QuestPart_Letter();
            successLetter.inSignal = timerCompleteSignal;
            successLetter.letter = LetterMaker.MakeLetter(
                "Military_Mission02_SuccessLabel".Translate(),
                "Military_Mission02_SuccessText".Translate(),
                LetterDefOf.PositiveEvent);
            quest.AddPart(successLetter);

            // Award 1000 silver
            QuestPart_AwardSilver reward = new QuestPart_AwardSilver();
            reward.inSignal = timerCompleteSignal;
            reward.amount = 5000;
            quest.AddPart(reward);

            // Remove Vane (guest leaves) and fire chain signal
            QuestPart_RemoveVane removeVane = new QuestPart_RemoveVane();
            removeVane.inSignal = timerCompleteSignal;
            removeVane.vanePawn = vane;
            removeVane.outSignalChain = chainSignal;
            quest.AddPart(removeVane);

            QuestPart_QuestEnd successEnd = new QuestPart_QuestEnd();
            successEnd.inSignal = timerCompleteSignal;
            successEnd.outcome = QuestEndOutcome.Success;
            quest.AddPart(successEnd);

            // ── FAIL: Vane died ──────────────────────────────────
            QuestPart_Letter failLetter = new QuestPart_Letter();
            failLetter.inSignal = failSignal;
            failLetter.letter = LetterMaker.MakeLetter(
                "Military_Mission02_FailLabel".Translate(),
                "Military_Mission02_FailText".Translate(),
                LetterDefOf.NegativeEvent);
            quest.AddPart(failLetter);

            QuestPart_QuestEnd failEnd = new QuestPart_QuestEnd();
            failEnd.inSignal = failSignal;
            failEnd.outcome = QuestEndOutcome.Fail;
            quest.AddPart(failEnd);

            if (Prefs.DevMode)
                Log.Message($"[Military] Mission 2 quest initialized: Vane={vane.LabelShort}, VIP shield active");
        }

        private Pawn SpawnVane(Map map)
        {
            PawnKindDef vaneKind = DefDatabase<PawnKindDef>.GetNamed("SilasVane_Guest", false);
            if (vaneKind == null)
            {
                Log.Error("[Military] QuestNode_Mission02: SilasVane_Guest pawn kind not found!");
                return null;
            }

            Pawn vane = PawnGenerator.GeneratePawn(vaneKind, Faction.OfPlayer);
            if (vane == null)
                return null;

            // Spawn near the colony center rather than the map edge.
            // Spawning at the edge causes all idle colonists to rush to the far side
            // of the map to "greet" the new pawn — instead Vane appears close to
            // where the squad already is and walks to them naturally.
            IntVec3 spot = CellFinder.RandomClosewalkCellNear(map.Center, map, 30, null);
            if (!spot.IsValid)
                spot = map.Center;

            GenSpawn.Spawn(vane, spot, map);

            // Set as VIP (so bodyguard mechanics can work immediately)
            MilitaryStatComp comp = MilitaryUtility.GetComp(vane);
            if (comp == null)
            {
                // Pawn doesn't have MilitaryStatComp, add manually via thingComps
                comp = new MilitaryStatComp();
                vane.AllComps.Add(comp);
                comp.parent = vane;
            }

            // Force his in-game name so he is clearly identifiable
            vane.Name = new NameTriple("Silas", "Vane", null);

            // Try to add a luxury outer layer if Royalty DLC is present.
            // We intentionally leave the PawnKindDef-generated clothes intact so
            // Vane always has clothing regardless of which DLCs are loaded.
            EquipLuxuryApparel(vane);

            if (Prefs.DevMode)
                Log.Message($"[Military] Silas Vane spawned at {spot}");

            return vane;
        }

        // Try to add a Masterwork royal robe over existing clothes (optional: requires Royalty DLC).
        // Does NOT strip the PawnKindDef-generated outfit, so Vane always has clothes.
        private static void EquipLuxuryApparel(Pawn pawn)
        {
            if (pawn.apparel == null) return;
            // Outer layer: prestige robe (Royalty DLC). Silently ignored if not available.
            TryEquipApparel(pawn, "Apparel_RobeRoyal", QualityCategory.Masterwork);
        }

        private static void TryEquipApparel(Pawn pawn, string defName, QualityCategory quality)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null) return;

            ThingDef mat = def.MadeFromStuff ? GenStuff.DefaultStuffFor(def) : null;
            Apparel apparel = (Apparel)ThingMaker.MakeThing(def, mat);
            apparel.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
            pawn.apparel.Wear(apparel, false);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return Find.AnyPlayerHomeMap != null;
        }
    }
}
