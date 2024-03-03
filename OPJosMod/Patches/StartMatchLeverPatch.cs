using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Config;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("PullLever")]
        [HarmonyPrefix]
        private static void pullLeverPatch(StartMatchLever __instance)
        {
            mls.LogMessage("player pulled lever, set them to seeker");
            PlayerControllerBPatch.isSeeker = true;
            PlayerControllerBPatch.isHider = false;
        }
    }
}
