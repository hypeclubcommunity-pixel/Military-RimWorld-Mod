using HarmonyLib;
using UnityEngine;
using Verse;

namespace Military
{
    public class MilitaryMod : Mod
    {
        public static MilitaryModSettings Settings { get; private set; }

        public MilitaryMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<MilitaryModSettings>();
            new Harmony("com.military.mod").PatchAll();
        }

        public override string SettingsCategory()
        {
            return "Military";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, MilitaryTheme.PanelBackground);
            Widgets.DrawBoxSolid(new Rect(inRect.x, inRect.y, inRect.width, 4f), MilitaryTheme.HeaderFill);
            Widgets.DrawBoxSolid(new Rect(inRect.x, inRect.yMax - 1f, inRect.width, 1f), MilitaryTheme.PanelTrim);

            Rect iconRect = new Rect(inRect.x + 10f, inRect.y + 10f, 22f, 22f);
            GUI.DrawTexture(iconRect, MilitaryTextures.MainButton);

            Text.Font = GameFont.Medium;
            GUI.color = MilitaryTheme.SectionTitle;
            Widgets.Label(new Rect(iconRect.xMax + 6f, inRect.y + 7f, 220f, 28f), "Military Settings");
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            Rect contentRect = new Rect(inRect.x + 12f, inRect.y + 44f, inRect.width - 24f, inRect.height - 56f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(contentRect);

            GUI.color = MilitaryTheme.TextMuted;
            listing.Label("Configure core military behavior.");
            GUI.color = Color.white;
            listing.GapLine();

            bool enableResponseSystem = Settings == null || Settings.enableResponseSystem;
            listing.CheckboxLabeled("Enable Military Response System", ref enableResponseSystem);
            if (Settings != null)
                Settings.enableResponseSystem = enableResponseSystem;

            listing.End();
        }
    }
}
