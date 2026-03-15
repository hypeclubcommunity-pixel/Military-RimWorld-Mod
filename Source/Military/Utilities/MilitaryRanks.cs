using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Military
{
    public static class MilitaryRanks
    {
        // Internal rank keys — these are stable strings used for save/load.
        // Do NOT translate these; use TranslatedName() for display.
        public static readonly List<string> All = new List<string>
        {
            "Recruit",
            "Private",
            "Corporal",
            "Sergeant",
            "Lieutenant"
        };

        // Map from internal key → translation key
        private static readonly Dictionary<string, string> TranslationKeys = new Dictionary<string, string>
        {
            { "Recruit",    "Military_Rank_Recruit" },
            { "Private",    "Military_Rank_Private" },
            { "Corporal",   "Military_Rank_Corporal" },
            { "Sergeant",   "Military_Rank_Sergeant" },
            { "Lieutenant", "Military_Rank_Lieutenant" }
        };

        public static string Default => All[0];

        public static readonly Dictionary<string, int> KillThresholds = new Dictionary<string, int>
        {
            { "Recruit", 0 },
            { "Private", 3 },
            { "Corporal", 8 },
            { "Sergeant", 18 },
            { "Lieutenant", 35 }
        };

        public static bool IsEligibleForRank(string rank, int kills)
        {
            return KillThresholds.TryGetValue(rank, out int threshold) && kills >= threshold;
        }

        public static string NextEligibleRank(string currentRank, int kills)
        {
            string next = Next(currentRank);
            if (next == currentRank)
                return null;
            return IsEligibleForRank(next, kills) ? next : null;
        }

        public static bool IsValid(string rank) => All.Contains(rank);

        public static string TranslatedName(string rank)
        {
            if (TranslationKeys.TryGetValue(rank, out string key))
                return key.Translate();
            return rank;
        }

        public static List<string> TranslatedAll =>
            All.Select(TranslatedName).ToList();

        public static string Next(string currentRank)
        {
            int index = All.IndexOf(currentRank);
            if (index < 0 || index >= All.Count - 1) return currentRank;
            return All[index + 1];
        }

        public static string Previous(string currentRank)
        {
            int index = All.IndexOf(currentRank);
            if (index <= 0) return currentRank;
            return All[index - 1];
        }
    }
}

