using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public static class MilitaryUtility
    {
        public static MilitaryStatComp GetComp(Pawn pawn)
        {
            return pawn?.TryGetComp<MilitaryStatComp>();
        }

        public static bool HasComp(Pawn pawn)
        {
            return GetComp(pawn) != null;
        }

        public static bool IsEligible(Pawn pawn)
        {
            if (pawn == null || pawn.health?.capacities == null || pawn.health?.hediffSet == null)
                return false;

            // Age: must be 18+
            if (pawn.ageTracker.AgeBiologicalYears < 18)
                return false;

            // Capacities: can move, can manipulate
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                return false;

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
                return false;

            // Sight: must have at least 10%
            if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) < 0.1f)
                return false;

            // Pregnancy
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.PregnantHuman))
                return false;

            // Missing limbs: no missing arms or legs
            if (pawn.health.hediffSet.hediffs.Any(h =>
                h is Hediff_MissingPart &&
                (h.Part.def == BodyPartDefOf.Leg || h.Part.def == BodyPartDefOf.Arm)))
                return false;

            // Permanent/chronic conditions
            if (pawn.health.hediffSet.hediffs.Any(h =>
                h.def.chronic || (h.def.makesSickThought && !h.def.tendable && h.IsPermanent())))
                return false;

            // Brain damage: permanent hediff on a part tagged as consciousness source
            if (pawn.health.hediffSet.hediffs.Any(h =>
                h.Part != null && h.Part.def.tags.Contains(BodyPartTagDefOf.ConsciousnessSource) && h.IsPermanent()))
                return false;

            // Pacifists cannot serve
            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
                return false;

            return true;
        }

        public static string GetRank(Pawn pawn)
        {
            return GetComp(pawn)?.rank ?? MilitaryRanks.Default;
        }

        public static void SetRank(Pawn pawn, string rank)
        {
            var comp = GetComp(pawn);
            if (comp != null && MilitaryRanks.IsValid(rank))
                comp.rank = rank;
        }

        public static void SendEligibilityLetter(Pawn pawn, string nextRank)
        {
            Find.LetterStack.ReceiveLetter(
                "Military_EligibleLabel".Translate(pawn.LabelShort),
                "Military_EligibleText".Translate(pawn.LabelShort, MilitaryRanks.TranslatedName(nextRank)),
                LetterDefOf.PositiveEvent,
                new LookTargets(pawn));
        }

        public static void SendPromotionLetter(Pawn pawn, string newRank)
        {
            Find.LetterStack.ReceiveLetter(
                "Military_PromotedLabel".Translate(),
                "Military_PromotedText".Translate(pawn.LabelShort, MilitaryRanks.TranslatedName(newRank)),
                LetterDefOf.PositiveEvent,
                new LookTargets(pawn));
        }

        public static Texture2D GetRankTexture(string rank)
        {
            return ContentFinder<Texture2D>.Get("Military/Ranks/" + rank, false) ?? BaseContent.BadTex;
        }
    }
}

