using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Military
{
    public class JobDriver_TrainCombat : JobDriver
    {
        private const int TrainDurationTicks = 2500; // ~1 hour in-game
        private const int MaxSessionsPerDay = 1;
        private const float BaseXpPerCycle = 150f;
        private const int RangedStandoff = 3; // cells from dummy for ranged

        private Building_CombatDummy Dummy => TargetThingA as Building_CombatDummy;

        private int ticksElapsed;
        private bool isRangedMode;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => Dummy?.GetCurrentDesignation() == null);

            Building_CombatDummy dummy = Dummy;
            bool allowsRanged = dummy?.AllowsRanged() ?? false;
            bool hasRangedWeapon = pawn.equipment?.Primary?.def.IsRangedWeapon ?? false;

            isRangedMode = hasRangedWeapon && allowsRanged;

            if (isRangedMode)
            {
                // RANGED: Walk to a cell in FRONT of the dummy, ~3 tiles away
                Toil gotoRanged = ToilMaker.MakeToil("GotoRangedPosition");
                gotoRanged.initAction = delegate
                {
                    IntVec3 frontCell = FindFrontCell(RangedStandoff);
                    if (frontCell.IsValid)
                        pawn.pather.StartPath(frontCell, PathEndMode.OnCell);
                    else
                        pawn.pather.StartPath(TargetA, PathEndMode.Touch);
                };
                gotoRanged.defaultCompleteMode = ToilCompleteMode.PatherArrival;
                gotoRanged.FailOnDespawnedNullOrForbidden(TargetIndex.A);
                yield return gotoRanged;
            }
            else
            {
                // MELEE: Walk to the cell directly in FRONT of the dummy (face-to-face)
                Toil gotoMelee = ToilMaker.MakeToil("GotoMeleePosition");
                gotoMelee.initAction = delegate
                {
                    IntVec3 frontCell = FindFrontCell(1);
                    if (frontCell.IsValid)
                        pawn.pather.StartPath(frontCell, PathEndMode.OnCell);
                    else
                        pawn.pather.StartPath(TargetA, PathEndMode.Touch);
                };
                gotoMelee.defaultCompleteMode = ToilCompleteMode.PatherArrival;
                gotoMelee.FailOnDespawnedNullOrForbidden(TargetIndex.A);
                yield return gotoMelee;
            }

            // TRAINING LOOP: attack the dummy repeatedly
            Toil trainLoop = ToilMaker.MakeToil("TrainCombatLoop");
            ticksElapsed = 0;

            trainLoop.initAction = delegate
            {
                pawn.rotationTracker.FaceTarget(TargetA);
            };

            trainLoop.tickAction = delegate
            {
                ticksElapsed++;
                pawn.rotationTracker.FaceTarget(TargetA);

                if (isRangedMode)
                    DoRangedTrainingTick();
                else
                    DoMeleeTrainingTick();

                if (ticksElapsed >= TrainDurationTicks)
                    ReadyForNextToil();
            };

            trainLoop.handlingFacing = true;
            trainLoop.defaultCompleteMode = ToilCompleteMode.Never;
            trainLoop.WithProgressBar(TargetIndex.A, () => (float)ticksElapsed / TrainDurationTicks);
            trainLoop.AddFinishAction(delegate
            {
                if (ticksElapsed >= TrainDurationTicks)
                    OnTrainingComplete();
            });
            yield return trainLoop;
        }

        /// <summary>
        /// Find a walkable cell in front of the dummy based on its rotation.
        /// Distance 1 = adjacent face-to-face, Distance 3 = ranged standoff.
        /// Falls back to nearby cells if the exact front cell is blocked.
        /// </summary>
        private IntVec3 FindFrontCell(int distance)
        {
            Building_CombatDummy dummy = Dummy;
            if (dummy == null) return IntVec3.Invalid;

            IntVec3 dummyPos = dummy.Position;
            Rot4 rot = dummy.Rotation;

            // The "front" of the dummy is the direction it faces
            IntVec3 forward = rot.FacingCell;
            IntVec3 targetCell = dummyPos + forward * distance;

            if (targetCell.InBounds(pawn.Map) && targetCell.Standable(pawn.Map)
                && pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                return targetCell;

            // Try cells to the left and right of the front position
            IntVec3 right = rot.Rotated(RotationDirection.Clockwise).FacingCell;
            for (int offset = 1; offset <= 2; offset++)
            {
                IntVec3 tryRight = targetCell + right * offset;
                if (tryRight.InBounds(pawn.Map) && tryRight.Standable(pawn.Map)
                    && pawn.CanReach(tryRight, PathEndMode.OnCell, Danger.Deadly))
                    return tryRight;

                IntVec3 tryLeft = targetCell - right * offset;
                if (tryLeft.InBounds(pawn.Map) && tryLeft.Standable(pawn.Map)
                    && pawn.CanReach(tryLeft, PathEndMode.OnCell, Danger.Deadly))
                    return tryLeft;
            }

            return IntVec3.Invalid;
        }

        private void DoMeleeTrainingTick()
        {
            if (pawn.stances?.curStance is Stance_Busy)
                return; // Warmup or cooldown in progress — let it play

            Thing target = TargetThingA;
            if (target == null || target.Destroyed)
                return;

            bool attacked = pawn.meleeVerbs.TryMeleeAttack(target, null, false);
            if (attacked)
            {
                AwardTrainingXp(SkillDefOf.Melee);
                HealDummy(); // Restore HP so the dummy never breaks
            }
        }

        private void DoRangedTrainingTick()
        {
            if (pawn.stances?.curStance is Stance_Busy)
                return;

            Thing target = TargetThingA;
            if (target == null || target.Destroyed)
                return;

            Verb verb = pawn.equipment?.PrimaryEq?.PrimaryVerb;
            if (verb == null || !verb.Available())
                return;

            if (!verb.IsMeleeAttack)
            {
                verb.TryStartCastOn(target, surpriseAttack: false, canHitNonTargetPawns: false);
                AwardTrainingXp(SkillDefOf.Shooting);
                HealDummy();
            }
        }

        /// <summary>
        /// Restore the dummy to full HP after each training hit.
        /// This prevents the dummy from ever breaking during training.
        /// </summary>
        private void HealDummy()
        {
            Building_CombatDummy dummy = Dummy;
            if (dummy != null && !dummy.Destroyed)
                dummy.HitPoints = dummy.MaxHitPoints;
        }

        private void AwardTrainingXp(SkillDef skill)
        {
            Building_CombatDummy dummy = Dummy;
            float materialMult = dummy?.GetMaterialMultiplier() ?? 1f;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            string rank = comp?.rank ?? "";
            float rankMult = MilitaryRanks.GetTrainingMultiplier(rank);

            float xp = BaseXpPerCycle * materialMult * rankMult;
            pawn.skills?.Learn(skill, xp, true);
        }

        private void OnTrainingComplete()
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp != null && !string.IsNullOrEmpty(comp.rank))
                comp.missionCount++;

            ApplyRoomThought();
            IncrementSessionCount();

            Messages.Message(
                "Military_TrainComplete".Translate(pawn.LabelShort),
                new LookTargets(pawn),
                MessageTypeDefOf.PositiveEvent,
                historical: false);
        }

        private void ApplyRoomThought()
        {
            Room room = pawn.GetRoom();
            if (room == null || room.Role != MilitaryTrainingDefOf.Military_CombatTrainingRoom)
                return;

            int impressiveness = Mathf.RoundToInt(room.GetStat(RoomStatDefOf.Impressiveness));
            int stage;
            if (impressiveness < 20) return;
            else if (impressiveness < 30) stage = 3;
            else if (impressiveness < 40) stage = 4;
            else if (impressiveness < 55) stage = 5;
            else if (impressiveness < 75) stage = 6;
            else if (impressiveness < 95) stage = 7;
            else if (impressiveness < 120) stage = 8;
            else stage = 9;

            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(
                ThoughtMaker.MakeThought(MilitaryTrainingDefOf.Military_TrainedInImpressiveRoom, stage));
        }

        public static bool HasTrainedEnoughToday(Pawn pawn)
        {
            int day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(pawn.MapHeld.Tile).x);
            var manager = GameComponent_MilitaryManager.Instance;
            if (manager == null) return false;

            int stored = manager.GetTrainingSessions(pawn.thingIDNumber, day);
            return stored >= MaxSessionsPerDay;
        }

        private void IncrementSessionCount()
        {
            int day = GenDate.DayOfYear(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(pawn.MapHeld.Tile).x);
            var manager = GameComponent_MilitaryManager.Instance;
            manager?.IncrementTrainingSessions(pawn.thingIDNumber, day);
        }
    }
}
