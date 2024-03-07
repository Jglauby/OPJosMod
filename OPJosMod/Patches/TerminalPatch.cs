using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Config;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void startPatch(Terminal __instance)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                __instance.groupCredits = 123456789;
                __instance.SyncGroupCreditsServerRpc(__instance.groupCredits, __instance.numberOfItemsInDropship);
            }
        }

        [HarmonyPatch("RunTerminalEvents")]
        [HarmonyPrefix]
        private static void runTerminalEventsPatch(Terminal __instance)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                __instance.groupCredits = 123456789;
                __instance.SyncGroupCreditsServerRpc(__instance.groupCredits, __instance.numberOfItemsInDropship);
            }
        }
    }
}
