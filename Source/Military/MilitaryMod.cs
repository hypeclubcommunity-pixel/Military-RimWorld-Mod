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
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            bool enableResponseSystem = Settings == null || Settings.enableResponseSystem;
            listing.CheckboxLabeled("Enable Military Response System", ref enableResponseSystem);
            if (Settings != null)
                Settings.enableResponseSystem = enableResponseSystem;

            listing.End();
        }
    }
}
