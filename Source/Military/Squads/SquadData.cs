using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Military
{
    public class SquadData : IExposable
    {
        public string squadId = Guid.NewGuid().ToString();
        public string squadName = "";
        public int leaderPawnId = -1;
        public List<int> memberPawnIds = new List<int>();

        public const int MaxMembers = 5;

        public bool IsLeaderless => leaderPawnId == -1;

        public Pawn GetLeader(Map map)
        {
            if (leaderPawnId == -1 || map == null)
                return null;

            Pawn pawn = FindPawnById(map, leaderPawnId, true);
            if (pawn != null)
                return pawn;

            return FindPawnById(map, leaderPawnId, false);
        }

        public List<Pawn> GetMembers(Map map)
        {
            List<Pawn> members = new List<Pawn>();
            if (map == null || memberPawnIds == null || memberPawnIds.Count == 0)
                return members;

            for (int i = 0; i < memberPawnIds.Count; i++)
            {
                int id = memberPawnIds[i];
                Pawn pawn = FindPawnById(map, id, true) ?? FindPawnById(map, id, false);
                if (pawn != null)
                    members.Add(pawn);
            }

            return members;
        }

        public static bool IsValidLeader(Pawn pawn)
        {
            if (pawn == null || pawn.Faction != Faction.OfPlayer)
                return false;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return false;

            return comp.rank == "Sergeant" || comp.rank == "Lieutenant";
        }

        public void AutoPromoteLeader(Map map)
        {
            int oldLeaderId = leaderPawnId;
            Pawn oldLeader = FindPawnById(map, oldLeaderId, true) ?? FindPawnById(map, oldLeaderId, false);

            Pawn bestPawn = null;
            int bestRankIndex = int.MinValue;
            int bestShooting = int.MinValue;

            if (memberPawnIds != null)
            {
                for (int i = 0; i < memberPawnIds.Count; i++)
                {
                    Pawn candidate = FindPawnById(map, memberPawnIds[i], true) ?? FindPawnById(map, memberPawnIds[i], false);
                    if (candidate == null || candidate.Faction != Faction.OfPlayer)
                        continue;

                    MilitaryStatComp comp = MilitaryUtility.GetComp(candidate);
                    if (comp == null || string.IsNullOrEmpty(comp.rank))
                        continue;

                    int rankIndex = MilitaryRanks.All.IndexOf(comp.rank);
                    if (rankIndex < 0)
                        continue;

                    int shooting = candidate.skills?.GetSkill(SkillDefOf.Shooting)?.Level ?? 0;
                    if (rankIndex > bestRankIndex || (rankIndex == bestRankIndex && shooting > bestShooting))
                    {
                        bestPawn = candidate;
                        bestRankIndex = rankIndex;
                        bestShooting = shooting;
                    }
                }
            }

            if (oldLeader != null)
            {
                MilitaryStatComp oldComp = MilitaryUtility.GetComp(oldLeader);
                if (oldComp != null)
                    oldComp.isSquadLeader = false;
            }

            if (bestPawn == null || !IsValidLeader(bestPawn))
            {
                leaderPawnId = -1;
                ClearAllMemberLeaderFlags(map);
                Log.Message($"[Military] Squad {squadName} has no eligible leader \u2014 squad is leaderless");
                return;
            }

            leaderPawnId = bestPawn.thingIDNumber;
            memberPawnIds?.Remove(bestPawn.thingIDNumber);

            MilitaryStatComp newComp = MilitaryUtility.GetComp(bestPawn);
            if (newComp != null)
            {
                newComp.squadId = squadId;
                newComp.isSquadLeader = true;
            }

            if (Prefs.DevMode)
                Log.Message($"[Military] {bestPawn.LabelShort} auto-promoted to leader of {squadName}");
        }

        private void ClearAllMemberLeaderFlags(Map map)
        {
            if (memberPawnIds == null)
                return;

            for (int i = 0; i < memberPawnIds.Count; i++)
            {
                Pawn member = FindPawnById(map, memberPawnIds[i], true) ?? FindPawnById(map, memberPawnIds[i], false);
                if (member != null)
                {
                    MilitaryStatComp comp = MilitaryUtility.GetComp(member);
                    if (comp != null)
                        comp.isSquadLeader = false;
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref squadId, "squadId", "");
            Scribe_Values.Look(ref squadName, "squadName", "");
            Scribe_Values.Look(ref leaderPawnId, "leaderPawnId", -1);
            Scribe_Collections.Look(ref memberPawnIds, "memberPawnIds", LookMode.Value);

            if (memberPawnIds == null)
                memberPawnIds = new List<int>();
        }

        private static Pawn FindPawnById(Map map, int pawnId, bool spawnedOnly)
        {
            if (map == null || pawnId < 0)
                return null;

            IReadOnlyList<Pawn> pawns = spawnedOnly ? map.mapPawns.AllPawnsSpawned : map.mapPawns.AllPawns;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn != null && pawn.thingIDNumber == pawnId)
                    return pawn;
            }

            return null;
        }
    }
}
