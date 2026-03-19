using RimWorld;
using Verse;

namespace Military
{
    [DefOf]
    public static class MilitaryTrainingDefOf
    {
        public static JobDef Military_TrainCombat;
        public static ThoughtDef Military_TrainedInImpressiveRoom;
        public static RoomRoleDef Military_CombatTrainingRoom;

        // Designations resolved manually because DefOf can't handle name collisions
        private static DesignationDef cachedTrainCombat;
        private static DesignationDef cachedTrainMelee;
        private static DesignationDef cachedTrainRanged;

        public static DesignationDef TrainCombatDes =>
            cachedTrainCombat ?? (cachedTrainCombat = DefDatabase<DesignationDef>.GetNamed("Military_TrainCombat"));
        public static DesignationDef TrainMeleeDes =>
            cachedTrainMelee ?? (cachedTrainMelee = DefDatabase<DesignationDef>.GetNamed("Military_TrainMelee"));
        public static DesignationDef TrainRangedDes =>
            cachedTrainRanged ?? (cachedTrainRanged = DefDatabase<DesignationDef>.GetNamed("Military_TrainRanged"));
    }
}
