using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class ResponseAssignmentRecord : IExposable
    {
        public int responderId = -1;
        public int colonistId = -1;
        public int hostileId = -1;
        public int lockUntilTick = -1;
        public int lastThreatTick = -1;

        public void ExposeData()
        {
            Scribe_Values.Look(ref responderId, "responderId", -1);
            Scribe_Values.Look(ref colonistId, "colonistId", -1);
            Scribe_Values.Look(ref hostileId, "hostileId", -1);
            Scribe_Values.Look(ref lockUntilTick, "lockUntilTick", -1);
            Scribe_Values.Look(ref lastThreatTick, "lastThreatTick", -1);
        }
    }

    public class ResponseCooldownRecord : IExposable
    {
        public int pawnId = -1;
        public int cooldownUntilTick = -1;

        public void ExposeData()
        {
            Scribe_Values.Look(ref pawnId, "pawnId", -1);
            Scribe_Values.Look(ref cooldownUntilTick, "cooldownUntilTick", -1);
        }
    }

    public class RecentDamageRecord : IExposable
    {
        public int victimId = -1;
        public int hostileId = -1;
        public int expiresAtTick = -1;

        public void ExposeData()
        {
            Scribe_Values.Look(ref victimId, "victimId", -1);
            Scribe_Values.Look(ref hostileId, "hostileId", -1);
            Scribe_Values.Look(ref expiresAtTick, "expiresAtTick", -1);
        }
    }

    public class SavedResponseStateRecord : IExposable
    {
        public int pawnId = -1;
        public bool wasDrafted;
        public bool hadPatrolAssignment;
        public bool wasPatrolling;
        public Job savedJob;

        public void ExposeData()
        {
            Scribe_Values.Look(ref pawnId, "pawnId", -1);
            Scribe_Values.Look(ref wasDrafted, "wasDrafted", false);
            Scribe_Values.Look(ref hadPatrolAssignment, "hadPatrolAssignment", false);
            Scribe_Values.Look(ref wasPatrolling, "wasPatrolling", false);
            Scribe_Deep.Look(ref savedJob, "savedJob");
        }
    }

    public class MilitaryResponseSystem : MapComponent
    {
        private const int TickInterval = 120;
        private const float ThreatRadius = 30f;
        private const int DamageWindowTicks = 600;
        private const int AssignmentLockTicks = 300;
        private const int CooldownTicks = 300;
        private const string DebugPrefix = "[Military][ResponseDebug]";

        private static readonly HashSet<JobDef> PatrolInterruptibleJobs = new HashSet<JobDef>
        {
            JobDefOf.Wait,
            JobDefOf.Wait_Wander,
            JobDefOf.GotoWander
        };
        private static readonly bool DebugMode = false;

        private List<ResponseAssignmentRecord> assignments = new List<ResponseAssignmentRecord>();
        private List<ResponseCooldownRecord> cooldowns = new List<ResponseCooldownRecord>();
        private List<RecentDamageRecord> recentDamage = new List<RecentDamageRecord>();
        private List<SavedResponseStateRecord> savedStates = new List<SavedResponseStateRecord>();

        public MilitaryResponseSystem(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            if (!ShouldRun())
            {
                ShutdownActiveAssignments(now);
                CleanupState(now);
                return;
            }

            if (now % TickInterval != 0)
                return;

            CleanupState(now);
            TickAssignments(now);
            AssignResponders(now);
        }

        public void RecordHostileDamage(Pawn victim, Pawn instigator, int currentTick)
        {
            if (!ShouldTrackColonist(victim) || !IsThreatPawn(instigator, victim))
                return;

            if (victim.Map != map || instigator.Map != map)
                return;

            UpsertDamageRecord(victim.thingIDNumber, instigator.thingIDNumber, currentTick + DamageWindowTicks);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref assignments, "assignments", LookMode.Deep);
            Scribe_Collections.Look(ref cooldowns, "cooldowns", LookMode.Deep);
            Scribe_Collections.Look(ref recentDamage, "recentDamage", LookMode.Deep);
            Scribe_Collections.Look(ref savedStates, "savedStates", LookMode.Deep);

            if (assignments == null)
                assignments = new List<ResponseAssignmentRecord>();
            if (cooldowns == null)
                cooldowns = new List<ResponseCooldownRecord>();
            if (recentDamage == null)
                recentDamage = new List<RecentDamageRecord>();
            if (savedStates == null)
                savedStates = new List<SavedResponseStateRecord>();
        }

        private bool ShouldRun()
        {
            if (MilitaryMod.Settings != null && !MilitaryMod.Settings.enableResponseSystem)
                return false;

            if (map == null || map.Disposed || !map.IsPlayerHome)
                return false;

            return true;
        }

        private void TickAssignments(int now)
        {
            for (int i = assignments.Count - 1; i >= 0; i--)
            {
                ResponseAssignmentRecord assignment = assignments[i];
                Pawn responder = FindPawnForAssignment(assignment.responderId);
                Pawn colonist = FindPawnForAssignment(assignment.colonistId);
                Pawn hostile = FindPawnForAssignment(assignment.hostileId);

                if (!CanContinueResponding(responder))
                {
                    AbortAssignment(i, responder);
                    continue;
                }

                if (!ShouldTrackColonist(colonist))
                {
                    AbortAssignment(i, responder);
                    continue;
                }

                if (!IsThreatPawn(hostile, colonist))
                {
                    FinishAssignment(i, responder, now, true);
                    continue;
                }

                bool threatStillActive = IsHostileThreateningColonist(hostile, colonist, now);
                bool activelyPursuingThreat = HasActiveResponseJobTargeting(responder, hostile);

                if (!activelyPursuingThreat && threatStillActive)
                {
                    RefreshResponseJob(responder, hostile, now);
                    activelyPursuingThreat = HasActiveResponseJobTargeting(responder, hostile);
                }

                if (activelyPursuingThreat || threatStillActive)
                {
                    assignment.lastThreatTick = now;
                    continue;
                }

                if (now < assignment.lockUntilTick)
                    continue;

                FinishAssignment(i, responder, now, true);
            }
        }

        private void AssignResponders(int now)
        {
            int endangeredColonistCount = 0;
            IReadOnlyList<Pawn> allPawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn colonist = allPawns[i];
                if (!ShouldTrackColonist(colonist))
                    continue;

                if (GetAssignmentByColonist(colonist.thingIDNumber) != null)
                    continue;

                Pawn hostile = GetPrimaryThreat(colonist, now, -1);
                if (hostile == null)
                    continue;

                endangeredColonistCount++;

                if (HasAssignmentForHostile(hostile.thingIDNumber))
                    continue;

                Pawn responder = FindClosestResponder(colonist, hostile, now);
                if (responder == null)
                    continue;

                TryAssignResponder(responder, colonist, hostile, now);
            }

        }

        private Pawn FindClosestResponder(Pawn colonist, Pawn hostile, int now)
        {
            Pawn best = null;
            float bestDistSq = float.MaxValue;
            int validResponderCount = 0;
            IReadOnlyList<Pawn> allPawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn candidate = allPawns[i];
                if (!CanBeSelectedAsResponder(candidate, colonist, hostile, now))
                    continue;

                validResponderCount++;

                float distSq = (candidate.Position - colonist.Position).LengthHorizontalSquared;
                if (best == null || distSq < bestDistSq)
                {
                    best = candidate;
                    bestDistSq = distSq;
                }
            }

            return best;
        }

        private bool CanBeSelectedAsResponder(Pawn pawn, Pawn colonist, Pawn hostile, int now)
        {
            bool isSelfResponse = pawn == colonist;

            if (!CanContinueResponding(pawn))
                return false;
            if (pawn == hostile)
                return false;
            if (pawn.Drafted)
                return false;
            if (IsSleepingOrEating(pawn))
                return false;
            if (IsAttackJob(pawn.CurJobDef))
                return false;
            if (HasAssignmentForResponder(pawn.thingIDNumber))
                return false;
            if (IsOnCooldown(pawn.thingIDNumber, now))
                return false;
            if (!isSelfResponse && IsEndangeredPawn(pawn, now))
                return false;
            if (!pawn.CanReach(hostile, PathEndMode.Touch, Danger.Deadly))
                return false;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (HasPatrolAssignment(comp) && IsBusyWithNonPatrolWork(pawn))
                return false;

            return true;
        }

        private bool CanContinueResponding(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.Map != map)
                return false;
            if (pawn.Faction != Faction.OfPlayer)
                return false;
            if (pawn.InMentalState || pawn.WorkTagIsDisabled(WorkTags.Violent))
                return false;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return false;
            if (comp.bodyguardTarget != null || comp.isDefending)
                return false;

            return true;
        }

        private bool ShouldTrackColonist(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.Map != map)
                return false;
            if (pawn.Faction != Faction.OfPlayer || !pawn.IsColonist)
                return false;
            if (pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony)
                return false;
            if (pawn.HostFaction != null || pawn.GuestStatus == GuestStatus.Guest)
                return false;

            return true;
        }

        private bool IsThreatPawn(Pawn pawn, Pawn colonist)
        {
            if (pawn == null || pawn.Dead || pawn.Downed || !pawn.Spawned || pawn.Map != map)
                return false;

            if (colonist != null && pawn == colonist)
                return false;

            if (pawn.Faction == Faction.OfPlayer && pawn.IsColonist)
                return false;

            return true;
        }

        private bool IsEndangeredPawn(Pawn pawn, int now)
        {
            return GetPrimaryThreat(pawn, now, -1) != null;
        }

        private Pawn GetPrimaryThreat(Pawn colonist, int now, int preferredHostileId)
        {
            if (!ShouldTrackColonist(colonist))
                return null;

            Pawn preferred = FindPawnForAssignment(preferredHostileId);
            if (IsHostileThreateningColonist(preferred, colonist, now))
                return preferred;

            Pawn best = null;
            float bestDistSq = float.MaxValue;
            IReadOnlyList<Pawn> allPawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn candidate = allPawns[i];
                if (!IsHostileThreateningColonist(candidate, colonist, now))
                    continue;

                float distSq = (candidate.Position - colonist.Position).LengthHorizontalSquared;
                if (best == null || distSq < bestDistSq)
                {
                    best = candidate;
                    bestDistSq = distSq;
                }
            }

            return best;
        }

        private bool IsHostileThreateningColonist(Pawn hostile, Pawn colonist, int now)
        {
            if (!ShouldTrackColonist(colonist) || !IsThreatPawn(hostile, colonist))
                return false;

            if (HasActiveAttackJobTargeting(hostile, colonist))
                return true;

            if (hostile.Position.InHorDistOf(colonist.Position, ThreatRadius)
                && hostile.CanReach(colonist, PathEndMode.Touch, Danger.Deadly)
                && (TargetsPawn(hostile.CurJob, colonist)
                    || hostile.mindState?.enemyTarget == colonist
                    || hostile.mindState?.meleeThreat == colonist))
            {
                return true;
            }

            return HasRecentDamage(hostile.thingIDNumber, colonist.thingIDNumber, now);
        }

        private static bool HasActiveAttackJobTargeting(Pawn hostile, Pawn colonist)
        {
            if (!IsAttackJob(hostile.CurJobDef))
                return false;

            return TargetsPawn(hostile.CurJob, colonist);
        }

        private static bool HasActiveResponseJobTargeting(Pawn responder, Pawn hostile)
        {
            if (responder == null || hostile == null || responder.jobs == null)
                return false;

            if (!IsAttackJob(responder.CurJobDef))
                return false;

            return TargetsPawn(responder.CurJob, hostile);
        }

        private static bool TargetsPawn(Job job, Pawn pawn)
        {
            if (job == null || pawn == null)
                return false;

            return job.targetA.Thing == pawn || job.targetB.Thing == pawn;
        }

        private static bool IsAttackJob(JobDef jobDef)
        {
            return jobDef == JobDefOf.AttackMelee || jobDef == JobDefOf.AttackStatic;
        }

        private static bool IsSleepingOrEating(Pawn pawn)
        {
            JobDef curJobDef = pawn.CurJobDef;
            return curJobDef == JobDefOf.LayDown || curJobDef == JobDefOf.Ingest;
        }

        private static bool HasPatrolAssignment(MilitaryStatComp comp)
        {
            return comp != null && comp.patrolWaypoints != null && comp.patrolWaypoints.Count >= 2;
        }

        private static bool IsBusyWithNonPatrolWork(Pawn pawn)
        {
            JobDef curJobDef = pawn.CurJobDef;
            if (curJobDef == null)
                return false;
            if (curJobDef == MilitaryJobDefOf.MilitaryPatrol)
                return false;

            return !PatrolInterruptibleJobs.Contains(curJobDef);
        }

        private void TryAssignResponder(Pawn responder, Pawn colonist, Pawn hostile, int now)
        {
            SavedResponseStateRecord savedState = CaptureState(responder);
            ReplaceSavedState(savedState);

            MilitaryStatComp comp = MilitaryUtility.GetComp(responder);
            if (savedState.wasPatrolling && comp != null)
                comp.isPatrolling = false;

            if (responder.drafter != null)
                responder.drafter.Drafted = true;

            Job attackJob = MakeResponseJob(responder, hostile);
            if (attackJob == null || responder.jobs == null || !responder.jobs.TryTakeOrderedJob(attackJob))
            {
                if (responder.drafter != null)
                    responder.drafter.Drafted = savedState.wasDrafted;
                if (savedState.wasPatrolling && comp != null)
                    comp.isPatrolling = true;
                ClearSavedState(responder.thingIDNumber);
                return;
            }

            LogDebug(
                $"{DebugPrefix} Responder drafted and sent: responder='{responder.LabelShort}', " +
                $"target='{hostile.LabelShort}', job='{attackJob.def.defName}', tick={now}");

            ResponseAssignmentRecord assignment = new ResponseAssignmentRecord
            {
                responderId = responder.thingIDNumber,
                colonistId = colonist.thingIDNumber,
                hostileId = hostile.thingIDNumber,
                lockUntilTick = now + AssignmentLockTicks,
                lastThreatTick = now
            };
            assignments.Add(assignment);
        }

        private void RefreshResponseJob(Pawn responder, Pawn hostile, int now)
        {
            if (responder == null || hostile == null || responder.jobs == null)
                return;

            if (HasActiveResponseJobTargeting(responder, hostile))
                return;

            if (responder.drafter != null)
                responder.drafter.Drafted = true;

            Job attackJob = MakeResponseJob(responder, hostile);
            if (attackJob == null || !responder.jobs.TryTakeOrderedJob(attackJob))
                return;

            LogDebug(
                $"{DebugPrefix} Refreshed response attack: responder='{responder.LabelShort}', " +
                $"target='{hostile.LabelShort}', job='{attackJob.def.defName}', tick={now}");
        }

        private static void LogDebug(string message)
        {
            if (DebugMode)
                Log.Message(message);
        }

        private SavedResponseStateRecord CaptureState(Pawn responder)
        {
            SavedResponseStateRecord state = new SavedResponseStateRecord();
            MilitaryStatComp comp = MilitaryUtility.GetComp(responder);
            state.pawnId = responder.thingIDNumber;
            state.wasDrafted = responder.Drafted;
            state.hadPatrolAssignment = HasPatrolAssignment(comp);
            state.wasPatrolling = state.hadPatrolAssignment
                && (comp.isPatrolling || responder.CurJobDef == MilitaryJobDefOf.MilitaryPatrol);

            // Job.Clone() is the smallest safe unit we can persist and replay later.
            Job currentJob = responder.CurJob;
            if (currentJob != null && !IsAttackJob(currentJob.def) && currentJob.def != MilitaryJobDefOf.MilitaryPatrol)
                state.savedJob = currentJob.Clone();

            return state;
        }

        private static Job MakeResponseJob(Pawn responder, Pawn hostile)
        {
            if (responder == null || hostile == null)
                return null;

            if (responder.equipment?.Primary?.def.IsRangedWeapon ?? false)
            {
                Job rangedJob = JobMaker.MakeJob(JobDefOf.AttackStatic, hostile);
                rangedJob.locomotionUrgency = LocomotionUrgency.Jog;
                return rangedJob;
            }

            return JobMaker.MakeJob(JobDefOf.AttackMelee, hostile);
        }

        private void FinishAssignment(int index, Pawn responder, int now, bool applyCooldown)
        {
            ResponseAssignmentRecord assignment = assignments[index];
            SavedResponseStateRecord savedState = GetSavedState(assignment.responderId);

            if (responder != null && responder.Spawned && !responder.Dead && !responder.Downed && responder.Map == map)
            {
                if (responder.drafter != null)
                    responder.drafter.Drafted = savedState != null && savedState.wasDrafted;

                bool restored = false;
                if (savedState != null)
                {
                    if (savedState.wasPatrolling && savedState.hadPatrolAssignment)
                        restored = TryRestorePatrol(responder);

                    if (!restored)
                        restored = TryRestoreSavedJob(responder, savedState);
                }

                if (!restored && responder.drafter != null && (savedState == null || !savedState.wasDrafted))
                    responder.drafter.Drafted = false;
            }

            if (applyCooldown && responder != null && responder.Spawned && !responder.Dead)
                SetCooldown(responder.thingIDNumber, now + CooldownTicks);

            ClearSavedState(assignment.responderId);
            assignments.RemoveAt(index);
        }

        private void AbortAssignment(int index, Pawn responder)
        {
            ResponseAssignmentRecord assignment = assignments[index];
            if (responder != null && responder.Spawned && !responder.Dead && !responder.Downed && responder.Map == map)
            {
                if (responder.drafter != null)
                    responder.drafter.Drafted = false;
            }

            ClearSavedState(assignment.responderId);
            assignments.RemoveAt(index);
        }

        private bool TryRestorePatrol(Pawn responder)
        {
            if (responder == null || responder.jobs == null || responder.Dead || responder.Downed || !responder.Spawned || responder.Map != map)
                return false;

            MilitaryStatComp comp = MilitaryUtility.GetComp(responder);
            if (!HasPatrolAssignment(comp))
                return false;

            comp.isPatrolling = true;
            Job patrolJob = JobMaker.MakeJob(MilitaryJobDefOf.MilitaryPatrol);
            patrolJob.locomotionUrgency = LocomotionUrgency.Walk;
            bool started = responder.jobs.TryTakeOrderedJob(patrolJob);
            if (!started)
                comp.isPatrolling = false;

            return started;
        }

        private static bool TryRestoreSavedJob(Pawn responder, SavedResponseStateRecord savedState)
        {
            if (responder == null || responder.jobs == null || savedState == null || savedState.savedJob == null)
                return false;

            Job restoredJob = savedState.savedJob.Clone();
            if (restoredJob == null || !restoredJob.CanBeginNow(responder, true))
                return false;

            return responder.jobs.TryTakeOrderedJob(restoredJob);
        }

        private void ShutdownActiveAssignments(int now)
        {
            for (int i = assignments.Count - 1; i >= 0; i--)
            {
                Pawn responder = FindPawnForAssignment(assignments[i].responderId);
                FinishAssignment(i, responder, now, false);
            }
        }

        private void CleanupState(int now)
        {
            for (int i = recentDamage.Count - 1; i >= 0; i--)
            {
                if (recentDamage[i] == null || recentDamage[i].expiresAtTick <= now)
                    recentDamage.RemoveAt(i);
            }

            for (int i = cooldowns.Count - 1; i >= 0; i--)
            {
                if (cooldowns[i] == null || cooldowns[i].cooldownUntilTick <= now)
                    cooldowns.RemoveAt(i);
            }

            HashSet<int> seenResponders = new HashSet<int>();
            HashSet<int> seenColonists = new HashSet<int>();
            HashSet<int> seenHostiles = new HashSet<int>();
            for (int i = assignments.Count - 1; i >= 0; i--)
            {
                ResponseAssignmentRecord assignment = assignments[i];
                bool invalid = assignment == null
                    || assignment.responderId < 0
                    || assignment.colonistId < 0
                    || assignment.hostileId < 0
                    || !seenResponders.Add(assignment.responderId)
                    || !seenColonists.Add(assignment.colonistId)
                    || !seenHostiles.Add(assignment.hostileId);

                if (!invalid)
                    continue;

                if (assignment != null)
                {
                    Pawn responder = FindPawnForAssignment(assignment.responderId);
                    if (responder != null && responder.Spawned && !responder.Dead && !responder.Downed && responder.Map == map)
                    {
                        if (responder.drafter != null)
                            responder.drafter.Drafted = false;
                    }

                    ClearSavedState(assignment.responderId);
                }

                assignments.RemoveAt(i);
            }

            for (int i = savedStates.Count - 1; i >= 0; i--)
            {
                if (savedStates[i] == null || !HasAssignmentForResponder(savedStates[i].pawnId))
                    savedStates.RemoveAt(i);
            }
        }

        private void UpsertDamageRecord(int victimId, int hostileId, int expiresAtTick)
        {
            for (int i = 0; i < recentDamage.Count; i++)
            {
                RecentDamageRecord record = recentDamage[i];
                if (record != null && record.victimId == victimId && record.hostileId == hostileId)
                {
                    record.expiresAtTick = expiresAtTick;
                    return;
                }
            }

            RecentDamageRecord newRecord = new RecentDamageRecord
            {
                victimId = victimId,
                hostileId = hostileId,
                expiresAtTick = expiresAtTick
            };
            recentDamage.Add(newRecord);
        }

        private bool HasRecentDamage(int hostileId, int victimId, int now)
        {
            for (int i = 0; i < recentDamage.Count; i++)
            {
                RecentDamageRecord record = recentDamage[i];
                if (record != null
                    && record.hostileId == hostileId
                    && record.victimId == victimId
                    && record.expiresAtTick > now)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsOnCooldown(int pawnId, int now)
        {
            for (int i = 0; i < cooldowns.Count; i++)
            {
                ResponseCooldownRecord record = cooldowns[i];
                if (record != null && record.pawnId == pawnId && record.cooldownUntilTick > now)
                    return true;
            }

            return false;
        }

        private void SetCooldown(int pawnId, int untilTick)
        {
            for (int i = 0; i < cooldowns.Count; i++)
            {
                ResponseCooldownRecord record = cooldowns[i];
                if (record != null && record.pawnId == pawnId)
                {
                    record.cooldownUntilTick = untilTick;
                    return;
                }
            }

            ResponseCooldownRecord newRecord = new ResponseCooldownRecord
            {
                pawnId = pawnId,
                cooldownUntilTick = untilTick
            };
            cooldowns.Add(newRecord);
        }

        private ResponseAssignmentRecord GetAssignmentByColonist(int colonistId)
        {
            for (int i = 0; i < assignments.Count; i++)
            {
                ResponseAssignmentRecord assignment = assignments[i];
                if (assignment != null && assignment.colonistId == colonistId)
                    return assignment;
            }

            return null;
        }

        private bool HasAssignmentForResponder(int responderId)
        {
            for (int i = 0; i < assignments.Count; i++)
            {
                ResponseAssignmentRecord assignment = assignments[i];
                if (assignment != null && assignment.responderId == responderId)
                    return true;
            }

            return false;
        }

        private bool HasAssignmentForHostile(int hostileId)
        {
            for (int i = 0; i < assignments.Count; i++)
            {
                ResponseAssignmentRecord assignment = assignments[i];
                if (assignment != null && assignment.hostileId == hostileId)
                    return true;
            }

            return false;
        }

        private SavedResponseStateRecord GetSavedState(int pawnId)
        {
            for (int i = 0; i < savedStates.Count; i++)
            {
                SavedResponseStateRecord state = savedStates[i];
                if (state != null && state.pawnId == pawnId)
                    return state;
            }

            return null;
        }

        private void ReplaceSavedState(SavedResponseStateRecord state)
        {
            ClearSavedState(state.pawnId);
            savedStates.Add(state);
        }

        private void ClearSavedState(int pawnId)
        {
            for (int i = savedStates.Count - 1; i >= 0; i--)
            {
                SavedResponseStateRecord state = savedStates[i];
                if (state != null && state.pawnId == pawnId)
                    savedStates.RemoveAt(i);
            }
        }

        private static Pawn FindPawnForAssignment(int pawnId)
        {
            if (pawnId < 0)
                return null;

            return MilitaryUtility.FindPawnGlobal(pawnId);
        }
    }
}
