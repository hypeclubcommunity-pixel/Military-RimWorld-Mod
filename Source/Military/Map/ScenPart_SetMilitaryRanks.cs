using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Military
{
    public class ScenPart_SetMilitaryRanks : ScenPart
    {
        public override void PostGameStart()
        {
            base.PostGameStart();

            if (Find.Maps == null)
                return;

            List<Pawn> colonists = new List<Pawn>();
            for (int i = 0; i < Find.Maps.Count; i++)
            {
                Map map = Find.Maps[i];
                if (map?.mapPawns == null)
                    continue;

                List<Pawn> free = map.mapPawns.FreeColonists;
                if (free == null)
                    continue;

                for (int j = 0; j < free.Count; j++)
                    colonists.Add(free[j]);
            }

            // Assign ranks by spawn order:
            // Index 0-1 → Lieutenant (35 kills)
            // Index 2+  → Sergeant  (18 kills)
            for (int i = 0; i < colonists.Count; i++)
            {
                Pawn pawn = colonists[i];
                MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
                if (comp == null)
                    continue;

                if (i <= 1)
                {
                    MilitaryUtility.SetRank(pawn, "Lieutenant");
                    comp.missionCount = 35;
                }
                else
                {
                    MilitaryUtility.SetRank(pawn, "Sergeant");
                    comp.missionCount = 18;
                }

                // Cleanup: remove hediffs that block IsEligible
                if (pawn.health?.hediffSet != null)
                {
                    List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                    for (int h = hediffs.Count - 1; h >= 0; h--)
                    {
                        Hediff hediff = hediffs[h];
                        if (hediff is Hediff_Injury)
                            continue;

                        // Remove missing parts (arms/legs block eligibility)
                        if (hediff is Hediff_MissingPart)
                        {
                            if (hediff.Part != null
                                && (hediff.Part.def == BodyPartDefOf.Leg
                                    || hediff.Part.def == BodyPartDefOf.Arm))
                            {
                                pawn.health.RemoveHediff(hediff);
                            }
                            continue;
                        }

                        // Remove blindness/deafness by defName
                        string defName = hediff.def.defName;
                        if (defName.Contains("Blind") || defName.Contains("Deaf"))
                        {
                            pawn.health.RemoveHediff(hediff);
                            continue;
                        }

                        if (!hediff.def.isBad)
                            continue;

                        bool shouldRemove = hediff.def.lethalSeverity > 0f
                            || hediff.def.chronic
                            || hediff.IsPermanent()
                            || (hediff.def.makesSickThought && !hediff.def.tendable)
                            || (hediff.Part != null
                                && hediff.Part.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource));

                        if (shouldRemove)
                            pawn.health.RemoveHediff(hediff);
                    }
                }

                // Cleanup: remove pacifist, wimp, violence-disabling, and psychically deaf traits
                if (pawn.story?.traits != null)
                {
                    List<Trait> traits = pawn.story.traits.allTraits;
                    for (int t = traits.Count - 1; t >= 0; t--)
                    {
                        Trait trait = traits[t];
                        if (trait.def.defName == "Pacifist" || trait.def.defName == "Wimp")
                        {
                            traits.RemoveAt(t);
                            continue;
                        }
                        if (trait.def.defName == "PsychicSensitivity" && trait.Degree == -1)
                        {
                            traits.RemoveAt(t);
                            continue;
                        }
                        if ((trait.def.disabledWorkTags & WorkTags.Violent) != 0)
                            traits.RemoveAt(t);
                    }
                }

                if (pawn.story != null && pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
                {
                    bool replacedBackstory = false;

                    if (pawn.story.Childhood != null && pawn.story.Childhood.workDisables.HasFlag(WorkTags.Violent))
                    {
                        pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefsListForReading
                            .Where(b => b.slot == BackstorySlot.Childhood
                                     && !b.workDisables.HasFlag(WorkTags.Violent))
                            .RandomElement();
                        replacedBackstory = true;
                    }

                    if (pawn.story.Adulthood != null && pawn.story.Adulthood.workDisables.HasFlag(WorkTags.Violent))
                    {
                        pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefsListForReading
                            .Where(b => b.slot == BackstorySlot.Adulthood
                                     && !b.workDisables.HasFlag(WorkTags.Violent))
                            .RandomElement();
                        replacedBackstory = true;
                    }

                    if (replacedBackstory)
                    {
                        pawn.Notify_DisabledWorkTypesChanged();

                        if (Prefs.DevMode)
                        {
                            bool eligibleAfterBackstoryCleanup = MilitaryUtility.IsEligible(pawn);
                            Log.Message($"[Military] {pawn.LabelShort} IsEligible after backstory cleanup: {eligibleAfterBackstoryCleanup}");
                        }
                    }
                }

                if (Prefs.DevMode)
                {
                    Log.Message($"[Military] Scenario rank set: {pawn.LabelShort} → {comp.rank} (kills: {comp.missionCount})");
                    bool eligible = MilitaryUtility.IsEligible(pawn);
                    Log.Message($"[Military] {pawn.LabelShort} IsEligible after cleanup: {eligible}");
                    if (!eligible)
                    {
                        if (pawn.ageTracker.AgeBiologicalYears < 18)
                            Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} age {pawn.ageTracker.AgeBiologicalYears} < 18");
                        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                            Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} incapable of Moving");
                        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                            Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} incapable of Manipulation");
                        if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < 0.1f)
                            Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} Sight {pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight):F2} < 0.10");
                        if (pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
                            Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} pregnant");
                        for (int h2 = 0; h2 < pawn.health.hediffSet.hediffs.Count; h2++)
                        {
                            Hediff hd = pawn.health.hediffSet.hediffs[h2];
                            if (hd is Hediff_MissingPart && hd.Part != null
                                && (hd.Part.def == BodyPartDefOf.Leg || hd.Part.def == BodyPartDefOf.Arm))
                                Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} missing {hd.Part.def.defName}");
                            if (hd.def.chronic)
                                Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} chronic hediff {hd.def.defName}");
                            if (hd.Part != null && hd.Part.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource) && hd.IsPermanent())
                                Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} permanent brain hediff {hd.def.defName}");
                        }
                        if (pawn.WorkTagIsDisabled(WorkTags.Violent))
                            Log.Warning($"[Military]   BLOCKED: {pawn.LabelShort} violence disabled");
                    }
                }
            }
        }
    }
}
