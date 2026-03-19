using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    [StaticConstructorOnStartup]
    public static class MilitaryUtility
    {
        static MilitaryUtility()
        {
            _vipShieldIcon = ContentFinder<Texture2D>.Get("Military/UI/VipShield", false) ?? BaseContent.BadTex;
            if (_vipShieldIcon != null)
                _vipShieldIcon.filterMode = FilterMode.Point;
        }
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

        private static readonly Texture2D _vipShieldIcon;
        public static Texture2D VipShieldIcon => _vipShieldIcon;

        public static Pawn FindPawnGlobal(int pawnId)
        {
            if (pawnId < 0)
                return null;

            if (Find.Maps != null)
            {
                for (int i = 0; i < Find.Maps.Count; i++)
                {
                    Map map = Find.Maps[i];
                    if (map?.mapPawns?.AllPawns == null)
                        continue;

                    IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawns;
                    for (int j = 0; j < pawns.Count; j++)
                    {
                        if (pawns[j] != null && pawns[j].thingIDNumber == pawnId)
                            return pawns[j];
                    }
                }
            }

            if (Find.WorldPawns != null)
            {
                List<Pawn> worldPawns = Find.WorldPawns.AllPawnsAliveOrDead;
                for (int i = 0; i < worldPawns.Count; i++)
                {
                    if (worldPawns[i] != null && worldPawns[i].thingIDNumber == pawnId)
                        return worldPawns[i];
                }
            }

            return null;
        }

        public static bool AssignBodyguard(Pawn bodyguard, Pawn vip)
        {
            if (bodyguard == null || vip == null || bodyguard == vip)
                return false;

            MilitaryStatComp bgComp = GetComp(bodyguard);
            MilitaryStatComp vipComp = GetComp(vip);
            if (bgComp == null || vipComp == null)
                return false;

            if (string.IsNullOrEmpty(bgComp.rank))
                return false;

            int rankIndex = MilitaryRanks.All.IndexOf(bgComp.rank);
            int sergeantIndex = MilitaryRanks.All.IndexOf("Sergeant");
            if (rankIndex < 0 || rankIndex > sergeantIndex)
                return false;

            if (vipComp.vipBodyguards.Count >= 2)
                return false;

            if (bgComp.bodyguardTarget != null)
                return false;

            bgComp.bodyguardTarget = vip;
            vipComp.vipBodyguards.Add(bodyguard);

            if (Prefs.DevMode) Log.Message($"[Military] {bodyguard.LabelShort} assigned as bodyguard for {vip.LabelShort}");
            return true;
        }

        public static void ClearBodyguard(Pawn bodyguard)
        {
            if (bodyguard == null)
                return;

            MilitaryStatComp bgComp = GetComp(bodyguard);
            if (bgComp == null)
                return;

            Pawn vip = bgComp.bodyguardTarget;
            if (vip != null)
            {
                MilitaryStatComp vipComp = GetComp(vip);
                vipComp?.vipBodyguards.Remove(bodyguard);
            }

            bgComp.bodyguardTarget = null;
            if (Prefs.DevMode) Log.Message($"[Military] {bodyguard.LabelShort} bodyguard assignment cleared");
        }

        public static void OnVipRemoved(Pawn vip)
        {
            if (vip == null)
                return;

            MilitaryStatComp vipComp = GetComp(vip);
            if (vipComp == null || vipComp.vipBodyguards == null || vipComp.vipBodyguards.Count == 0)
                return;

            for (int i = vipComp.vipBodyguards.Count - 1; i >= 0; i--)
            {
                Pawn bg = vipComp.vipBodyguards[i];
                if (bg != null)
                {
                    MilitaryStatComp bgComp = GetComp(bg);
                    if (bgComp != null)
                        bgComp.bodyguardTarget = null;
                }
            }

            vipComp.vipBodyguards.Clear();
            if (Prefs.DevMode) Log.Message($"[Military] VIP {vip.LabelShort} removed \u2014 all bodyguards cleared");
        }

        public static void AssignDefendArea(Pawn pawn, List<IntVec3> cells)
        {
            if (pawn == null || cells == null)
                return;

            MilitaryStatComp comp = GetComp(pawn);
            if (comp == null)
                return;

            comp.defendArea = new List<IntVec3>(cells);
            comp.isDefending = true;

            if (Prefs.DevMode) Log.Message($"[Military] {pawn.LabelShort} assigned to defend area ({cells.Count} cells)");
        }

        public static void ClearDefendArea(Pawn pawn)
        {
            if (pawn == null)
                return;

            MilitaryStatComp comp = GetComp(pawn);
            if (comp == null)
                return;

            comp.defendArea.Clear();
            comp.isDefending = false;

            if (Prefs.DevMode) Log.Message($"[Military] {pawn.LabelShort} defend area cleared");
        }

        public static void DropStacks(Map map, ThingDef def, int totalCount)
        {
            int limit = def.stackLimit > 0 ? def.stackLimit : totalCount;
            while (totalCount > 0)
            {
                int batch = System.Math.Min(totalCount, limit);
                Thing t = ThingMaker.MakeThing(def, null);
                t.stackCount = batch;
                GenThing.TryDropAndSetForbidden(t, map.Center, map, ThingPlaceMode.Near, out _, false);
                totalCount -= batch;
            }
        }
    }
}

