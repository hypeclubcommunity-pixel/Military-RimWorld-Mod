using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    [StaticConstructorOnStartup]
    public static class MilitaryTheme
    {
        // Core palette
        public static readonly Color NearBlack = new Color(0.07f, 0.08f, 0.07f);
        public static readonly Color DarkGreen = new Color(0.22f, 0.25f, 0.18f);
        public static readonly Color Olive = new Color(0.42f, 0.44f, 0.28f);
        public static readonly Color Brown = new Color(0.35f, 0.27f, 0.20f);
        public static readonly Color Beige = new Color(0.76f, 0.69f, 0.54f);

        // Readability-focused accents derived from the core palette.
        public static readonly Color TextPrimary = new Color(0.89f, 0.85f, 0.75f);
        public static readonly Color TextMuted = new Color(0.58f, 0.56f, 0.50f);
        public static readonly Color Disabled = new Color(0.42f, 0.41f, 0.37f);
        public static readonly Color AccentGreen = new Color(0.49f, 0.57f, 0.43f);
        public static readonly Color AccentOlive = new Color(0.63f, 0.67f, 0.46f);
        public static readonly Color AccentBrown = new Color(0.71f, 0.54f, 0.40f);
        public static readonly Color Bodyguard = new Color(0.70f, 0.66f, 0.50f);
        public static readonly Color PanelBackground = new Color(0.09f, 0.10f, 0.08f);
        public static readonly Color PanelFill = new Color(0.12f, 0.13f, 0.10f);
        public static readonly Color PanelTrim = new Color(0.31f, 0.25f, 0.18f);
        public static readonly Color HeaderFill = new Color(0.17f, 0.20f, 0.15f);
        public static readonly Color RowSelected = new Color(0.27f, 0.31f, 0.22f, 0.95f);
        public static readonly Color RowHover = new Color(0.19f, 0.22f, 0.16f, 0.95f);

        // Semantic usage
        public static readonly Color Promote = AccentOlive;
        public static readonly Color Demote = AccentBrown;
        public static readonly Color Warning = AccentBrown;
        public static readonly Color Patrol = AccentOlive;
        public static readonly Color Fighting = AccentBrown;
        public static readonly Color Idle = TextMuted;
        public static readonly Color Vip = Beige;
        public static readonly Color Defend = AccentGreen;

        public static readonly Color RankRecruit = Disabled;
        public static readonly Color RankPrivate = AccentOlive;
        public static readonly Color RankCorporal = AccentGreen;
        public static readonly Color RankSergeant = AccentBrown;
        public static readonly Color RankLieutenant = Beige;
        public static readonly Color SectionTitle = Beige;
        public static readonly Color NeutralButton = DarkGreen;

        public static readonly Texture2D RankProgressFill =
            SolidColorMaterials.NewSolidColorTexture(AccentOlive);

        public static readonly Texture2D RankProgressBackground =
            SolidColorMaterials.NewSolidColorTexture(NearBlack);

        public const string VipShieldTexturePath = "Military/UI/VipShield";
        public const string RankTextureRootPath = "Military/Ranks/";
    }
}
