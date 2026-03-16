using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Military
{
    public class GameComponent_MilitaryManager : GameComponent
    {
        public List<SquadData> squads = new List<SquadData>();

        public static GameComponent_MilitaryManager Instance =>
            Current.Game?.GetComponent<GameComponent_MilitaryManager>();

        public GameComponent_MilitaryManager()
        {
        }

        public GameComponent_MilitaryManager(Game game)
        {
        }

        public SquadData CreateSquad(string name, Pawn leader)
        {
            if (!SquadData.IsValidLeader(leader))
                return null;

            if (GetSquadOf(leader) != null)
                return null;

            MilitaryStatComp leaderComp = MilitaryUtility.GetComp(leader);
            if (leaderComp == null)
                return null;

            SquadData squad = new SquadData
            {
                squadId = Guid.NewGuid().ToString(),
                squadName = string.IsNullOrWhiteSpace(name) ? "Unnamed Squad" : name,
                leaderPawnId = leader.thingIDNumber,
                memberPawnIds = new List<int>()
            };

            squads.Add(squad);

            leaderComp.squadId = squad.squadId;
            leaderComp.isSquadLeader = true;

            if (Prefs.DevMode)
                Log.Message($"[Military] Squad {squad.squadName} created with leader {leader.LabelShort}");
            return squad;
        }

        public bool DisbandSquad(string squadId)
        {
            SquadData squad = GetSquadById(squadId);
            if (squad == null)
                return false;

            Pawn leader = FindPawnByIdGlobal(squad.leaderPawnId);
            if (leader != null)
                ClearSquadData(leader);

            if (squad.memberPawnIds != null)
            {
                for (int i = 0; i < squad.memberPawnIds.Count; i++)
                {
                    Pawn member = FindPawnByIdGlobal(squad.memberPawnIds[i]);
                    if (member != null)
                        ClearSquadData(member);
                }
            }

            squads.Remove(squad);
            if (Prefs.DevMode)
                Log.Message($"[Military] Squad {squad.squadName} disbanded");
            return true;
        }

        public bool AddMember(string squadId, Pawn pawn)
        {
            SquadData squad = GetSquadById(squadId);
            if (squad == null || pawn == null)
                return false;

            if (pawn.Faction != Faction.OfPlayer)
                return false;

            if (squad.memberPawnIds == null)
                squad.memberPawnIds = new List<int>();

            if (squad.memberPawnIds.Count >= SquadData.MaxMembers)
                return false;

            if (squad.memberPawnIds.Contains(pawn.thingIDNumber))
                return false;

            if (GetSquadOf(pawn) != null)
                return false;

            if (squad.leaderPawnId == pawn.thingIDNumber)
                return false;

            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return false;

            squad.memberPawnIds.Add(pawn.thingIDNumber);
            comp.squadId = squad.squadId;
            comp.isSquadLeader = false;

            if (Prefs.DevMode)
                Log.Message($"[Military] {pawn.LabelShort} added to squad {squad.squadName}");
            return true;
        }

        public bool RemoveMember(string squadId, Pawn pawn)
        {
            SquadData squad = GetSquadById(squadId);
            if (squad == null || pawn == null || squad.memberPawnIds == null)
                return false;

            if (!squad.memberPawnIds.Remove(pawn.thingIDNumber))
                return false;

            ClearSquadData(pawn);
            if (Prefs.DevMode)
                Log.Message($"[Military] {pawn.LabelShort} removed from squad {squad.squadName}");
            return true;
        }

        public SquadData GetSquadOf(Pawn pawn)
        {
            if (pawn == null || squads == null)
                return null;

            int pawnId = pawn.thingIDNumber;
            for (int i = 0; i < squads.Count; i++)
            {
                SquadData squad = squads[i];
                if (squad == null)
                    continue;

                if (squad.leaderPawnId == pawnId)
                    return squad;

                if (squad.memberPawnIds != null && squad.memberPawnIds.Contains(pawnId))
                    return squad;
            }

            return null;
        }

        public SquadData GetSquadById(string squadId)
        {
            if (string.IsNullOrEmpty(squadId) || squads == null)
                return null;

            for (int i = 0; i < squads.Count; i++)
            {
                SquadData squad = squads[i];
                if (squad != null && squad.squadId == squadId)
                    return squad;
            }

            return null;
        }

        public void OnPawnRemoved(Pawn pawn)
        {
            if (pawn == null)
                return;

            SquadData squad = GetSquadOf(pawn);

            if (squad != null)
            {
                if (squad.leaderPawnId == pawn.thingIDNumber)
                {
                    squad.AutoPromoteLeader(pawn.MapHeld ?? Find.CurrentMap);
                }
                else if (squad.memberPawnIds != null)
                {
                    squad.memberPawnIds.Remove(pawn.thingIDNumber);
                }
            }

            ClearSquadData(pawn);
            if (squad != null)
            {
                if (Prefs.DevMode)
                    Log.Message($"[Military] {pawn.LabelShort} removed from squad on pawn loss");
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref squads, "squads", LookMode.Deep);
            if (squads == null)
                squads = new List<SquadData>();
        }

        private static void ClearSquadData(Pawn pawn)
        {
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null)
                return;

            comp.squadId = "";
            comp.isSquadLeader = false;
        }

        private static Pawn FindPawnByIdGlobal(int pawnId)
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

                    List<Pawn> pawns = map.mapPawns.AllPawns;
                    for (int j = 0; j < pawns.Count; j++)
                    {
                        Pawn pawn = pawns[j];
                        if (pawn != null && pawn.thingIDNumber == pawnId)
                            return pawn;
                    }
                }
            }

            if (Find.WorldPawns != null)
            {
                List<Pawn> worldPawns = Find.WorldPawns.AllPawnsAliveOrDead;
                for (int i = 0; i < worldPawns.Count; i++)
                {
                    Pawn pawn = worldPawns[i];
                    if (pawn != null && pawn.thingIDNumber == pawnId)
                        return pawn;
                }
            }

            return null;
        }
    }
}
