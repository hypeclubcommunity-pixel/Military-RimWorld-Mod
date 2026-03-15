using HarmonyLib;
using Verse;

namespace Military
{
    public class MilitaryMod : Mod
    {
        public MilitaryMod(ModContentPack content) : base(content)
        {
            new Harmony("com.military.mod").PatchAll();
        }
    }
}
