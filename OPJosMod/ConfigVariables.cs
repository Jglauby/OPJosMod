using System;
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

        public static Key getButton(string buttonName)
        {
            Key key = (Key)Enum.Parse(typeof(Key), buttonName);
            return key;
        }
    }
}
