using BepInEx.Logging;
using System;
using System.Linq;
using UnityEngine.InputSystem;

namespace OPJosMod
{
    public static class ConfigVariables
    {
        public static float waitTimeBetweenInteractions;
        public static Key startGhostModeButton;
        public static Key teleportBodyButton;
        public static Key toggleBrightModeButton;
        public static Key teleportFrontDoorButton;
        public static Key switchToSpectateButton;
        public static Key toggleNoClipButton;
        public static float noClipSpeed;
        public static OPnessModes OPness;
        public static Key teleportShipButton;
        public static bool enemiesDetectYou;
        public static bool canPickupScrap;
        public static Key teleportToPlayerForwardButton;
        public static Key teleportToPlayerBackwardButton;
    }
}
