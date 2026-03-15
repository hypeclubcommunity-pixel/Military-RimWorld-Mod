using Military.Patches;
using Verse;

namespace Military
{
    [StaticConstructorOnStartup]
    public static class MilitaryStartup
    {
        static MilitaryStartup()
        {
            RankStatPatch.EnsureStatPartsInstalled();
        }
    }
}
