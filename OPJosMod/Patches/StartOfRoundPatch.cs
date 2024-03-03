using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("ReviveDeadPlayers")]
        [HarmonyPrefix]
        static void reviveDeadPlayersPatch(StartOfRound __instance)
        {
            mls.LogMessage("revive dead players patch hit in start of round class, reset isSeeker and isHider");

            PlayerControllerBPatch.isSeeker = false;
            PlayerControllerBPatch.isHider = false;
        }

        [HarmonyPatch("ShipLeave")]
        [HarmonyPrefix]
        public static void shipLeavePatch(StartOfRound __instance)
        {
            mls.LogMessage("ship leaving, reset isSeeker and isHider");

            PlayerControllerBPatch.isSeeker = false;
            PlayerControllerBPatch.isHider = false;
        }
    }
}
