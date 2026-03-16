using HarmonyLib;
using UnityEngine;
using Verse;

namespace Military
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawGUIOverlay))]
    public static class VipIndicator_DrawPatch
    {
        public static void Postfix(Pawn __instance)
        {
            if (!__instance.Spawned)
                return;

            MilitaryStatComp comp = MilitaryUtility.GetComp(__instance);
            if (comp == null || comp.vipBodyguardIds.Count == 0)
                return;

            // Only draw when zoomed in enough to see labels
            if (Find.CameraDriver.CurrentZoom > CameraZoomRange.Middle)
                return;

            Vector2 screenPos = UI.MapToUIPosition(__instance.DrawPos);

            // Position icon above pawn label (label sits ~20px below center, icon goes above it)
            Rect iconRect = new Rect(screenPos.x - 8f, screenPos.y - 38f, 16f, 16f);

            GUI.color = new Color(1f, 0.8f, 0f, 0.9f);
            GUI.DrawTexture(iconRect, MilitaryUtility.VipShieldIcon);
            GUI.color = Color.white;
        }
    }
}
