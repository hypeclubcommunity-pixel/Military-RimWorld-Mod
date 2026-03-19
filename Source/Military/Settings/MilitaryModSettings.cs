using Verse;

namespace Military
{
    public class MilitaryModSettings : ModSettings
    {
        public bool enableResponseSystem = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableResponseSystem, "enableResponseSystem", true);
        }
    }
}
