using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class Building_CombatDummy : Building
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            DesignationDef currentDes = GetCurrentDesignation();

            // Train Combat (both)
            yield return MakeDesignateGizmo(
                MilitaryTrainingDefOf.TrainCombatDes,
                "Military_TrainCombat_Label".Translate(),
                "Military_TrainCombat_Desc".Translate(),
                currentDes == MilitaryTrainingDefOf.TrainCombatDes);

            // Train Melee only
            yield return MakeDesignateGizmo(
                MilitaryTrainingDefOf.TrainMeleeDes,
                "Military_TrainMelee_Label".Translate(),
                "Military_TrainMelee_Desc".Translate(),
                currentDes == MilitaryTrainingDefOf.TrainMeleeDes);

            // Train Ranged only
            yield return MakeDesignateGizmo(
                MilitaryTrainingDefOf.TrainRangedDes,
                "Military_TrainRanged_Label".Translate(),
                "Military_TrainRanged_Desc".Translate(),
                currentDes == MilitaryTrainingDefOf.TrainRangedDes);

            // Cancel
            if (currentDes != null)
            {
                yield return new Command_Action
                {
                    icon = MilitaryTextures.CancelTraining,
                    defaultLabel = "Military_TrainCancel_Label".Translate(),
                    defaultDesc = "Military_TrainCancel_Desc".Translate(),
                    action = () => ClearDesignation()
                };
            }
        }

        private Command_Action MakeDesignateGizmo(DesignationDef desDef, string label,
            string desc, bool active)
        {
            var cmd = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get(desDef.texturePath, false) ?? BaseContent.BadTex,
                defaultLabel = label,
                defaultDesc = desc,
                action = () =>
                {
                    ClearDesignation();
                    Map.designationManager.AddDesignation(
                        new Designation(this, desDef));
                }
            };
            if (active)
                cmd.Disable("Already set");
            return cmd;
        }

        public DesignationDef GetCurrentDesignation()
        {
            if (Map == null) return null;

            var des = Map.designationManager.DesignationOn(this,
                MilitaryTrainingDefOf.TrainCombatDes);
            if (des != null) return des.def;

            des = Map.designationManager.DesignationOn(this,
                MilitaryTrainingDefOf.TrainMeleeDes);
            if (des != null) return des.def;

            des = Map.designationManager.DesignationOn(this,
                MilitaryTrainingDefOf.TrainRangedDes);
            if (des != null) return des.def;

            return null;
        }

        private void ClearDesignation()
        {
            if (Map == null) return;
            Map.designationManager.RemoveAllDesignationsOn(this);
        }

        public bool AllowsMelee()
        {
            DesignationDef d = GetCurrentDesignation();
            return d == MilitaryTrainingDefOf.TrainCombatDes
                || d == MilitaryTrainingDefOf.TrainMeleeDes;
        }

        public bool AllowsRanged()
        {
            DesignationDef d = GetCurrentDesignation();
            return d == MilitaryTrainingDefOf.TrainCombatDes
                || d == MilitaryTrainingDefOf.TrainRangedDes;
        }

        public float GetMaterialMultiplier()
        {
            if (Stuff == null) return 1.0f;
            if (Stuff.IsStuff)
            {
                foreach (var cat in Stuff.stuffProps.categories)
                {
                    if (cat == StuffCategoryDefOf.Metallic) return 1.3f;
                    if (cat == StuffCategoryDefOf.Stony) return 1.15f;
                }
            }
            return 1.0f; // Wood and anything else
        }
    }
}
