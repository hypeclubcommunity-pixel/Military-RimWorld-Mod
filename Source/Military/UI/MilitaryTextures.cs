using UnityEngine;
using Verse;

namespace Military
{
    [StaticConstructorOnStartup]
    public static class MilitaryTextures
    {
        public static readonly Texture2D Promote =
            ContentFinder<Texture2D>.Get("Military/Gizmos/Promote", false) ?? BaseContent.BadTex;

        public static readonly Texture2D Demote =
            ContentFinder<Texture2D>.Get("Military/Gizmos/Demote", false) ?? BaseContent.BadTex;

        public static readonly Texture2D AssignPatrol =
            ContentFinder<Texture2D>.Get("Military/Gizmos/AssignPatrol", false) ?? BaseContent.BadTex;

        public static readonly Texture2D StopPatrol =
            ContentFinder<Texture2D>.Get("Military/Gizmos/StopPatrol", false) ?? BaseContent.BadTex;

        public static readonly Texture2D AssignBodyguard =
            ContentFinder<Texture2D>.Get("Military/Gizmos/AssignBodyguard", false) ?? BaseContent.BadTex;

        public static readonly Texture2D StopBodyguard =
            ContentFinder<Texture2D>.Get("Military/Gizmos/StopBodyguard", false) ?? BaseContent.BadTex;

        public static readonly Texture2D AssignDefend =
            ContentFinder<Texture2D>.Get("Military/Gizmos/AssignDefend", false) ?? BaseContent.BadTex;

        public static readonly Texture2D StopDefend =
            ContentFinder<Texture2D>.Get("Military/Gizmos/StopDefend", false) ?? BaseContent.BadTex;

        public static readonly Texture2D CancelTraining =
            ContentFinder<Texture2D>.Get("Military/Gizmos/CancelTraining", false) ?? BaseContent.BadTex;

        public static readonly Texture2D PatrolColumn =
            ContentFinder<Texture2D>.Get("Military/Gizmos/PatrolColumn", false) ?? BaseContent.BadTex;

        public static readonly Texture2D MainButton =
            ContentFinder<Texture2D>.Get("Military/UI/MainButton_Military", false) ?? BaseContent.BadTex;
    }
}
