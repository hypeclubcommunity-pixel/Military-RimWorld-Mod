using System.Linq;
using RimWorld;
using Verse;

namespace Military
{
    public class QuestPart_SpawnRaid : QuestPart
    {
        public string inSignal;
        public float points;
        public int mapTile;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag != inSignal)
                return;

            Map map = Find.Maps.FirstOrDefault(m => m.Tile == mapTile);
            if (map == null)
                return;

            Faction faction = Find.FactionManager.AllFactions
                .Where(f => f.HostileTo(Faction.OfPlayer)
                          && !f.defeated
                          && f.def.humanlikeFaction)
                .RandomElementWithFallback(null);

            if (faction == null)
            {
                if (Prefs.DevMode)
                    Log.Warning("[Military] QuestPart_SpawnRaid: No hostile humanlike faction found — raid skipped");
                return;
            }

            IncidentParms parms = new IncidentParms();
            parms.target = map;
            parms.faction = faction;
            parms.points = points;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref points, "points");
            Scribe_Values.Look(ref mapTile, "mapTile");
        }
    }
}
