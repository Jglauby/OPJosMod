using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static bool isSeeker = false;
        public static bool isHider = true;

        public static void SetupHider()
        {
            PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
        }

        public static void SetupSeeker()
        {

        }
    }
}
