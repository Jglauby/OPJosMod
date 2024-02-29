using System;
using System.Linq;
using UnityEngine.InputSystem;

namespace OPJosMod
{
    public static class ConfigVariables
    {
        public static float waitTimeBetweenInteractions;
        public static string startGhostModeButton;
        public static string teleportBodyButton;
        public static string toggleBrightModeButton;
        public static string teleportFrontDoorButton;
        public static string switchToSpectateButton;
        public static string toggleNoClipButton;
        public static float noClipSpeed;
        public static string OPness;

        public static Key getButton(string buttonName)
        {
            Key key = (Key)Enum.Parse(typeof(Key), buttonName);
            return key;
        }

        public static OPnessModes getOPness()
        {
            switch (OPness.ToLower())
            {
                case "limited":
                    return OPnessModes.Limited;
                case "balanced":
                    return OPnessModes.Balanced;
                case "unrestricted":
                    return OPnessModes.Unrestricted;
                default:
                    return OPnessModes.Balanced;
            }
        }
    }
}
