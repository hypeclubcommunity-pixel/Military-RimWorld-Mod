using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Military
{
    public class WorkGiver_TrainCombat : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(ThingDef.Named("Military_CombatDummy"));

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_CombatDummy dummy))
                return false;

            if (dummy.GetCurrentDesignation() == null)
                return false;

            if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
                return false;

            // Must have a military rank
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            if (comp == null || string.IsNullOrEmpty(comp.rank))
                return false;

            // Check daily cap
            if (JobDriver_TrainCombat.HasTrainedEnoughToday(pawn))
                return false;

            // Check weapon compatibility with training mode
            bool hasRangedWeapon = pawn.equipment?.Primary?.def.IsRangedWeapon ?? false;
            bool hasMeleeWeapon = !hasRangedWeapon; // unarmed or melee weapon

            // Ranged-only designation requires a ranged weapon
            if (dummy.AllowsRanged() && !dummy.AllowsMelee() && !hasRangedWeapon)
                return false;

            // Melee-only designation requires a melee weapon or unarmed
            if (dummy.AllowsMelee() && !dummy.AllowsRanged() && !hasMeleeWeapon)
                return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(MilitaryTrainingDefOf.Military_TrainCombat, t);
        }
    }
}
