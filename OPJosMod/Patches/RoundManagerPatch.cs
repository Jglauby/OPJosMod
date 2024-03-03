using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Config;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("FinishGeneratingNewLevelClientRpc")]
        [HarmonyPostfix]
        private static void finishGeneratingNewLevelClientRpcPatch(RoundManager __instance)
        {
            mls.LogMessage("FinishGeneratingNewLevelClientRpc patch hit");

            //if the round starts and you arent set as seeker, then you didnt pull the lever and you should be a hider
            if (PlayerControllerBPatch.isSeeker)
                PlayerControllerBPatch.SetupSeeker();
            else
                PlayerControllerBPatch.SetupHider();
        }

        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        private static void loadNewLevelPatch(RoundManager __instance)
        {
            mls.LogMessage("load new level patch hit");

            //GameNetworkManager.Instance.isHostingGame
            __instance.currentMaxInsidePower = 0;
            __instance.currentMaxOutsidePower = 0;
            __instance.currentLevel.maxEnemyPowerCount = 0;
            __instance.currentLevel.maxOutsideEnemyPowerCount = 0;

            //make no scrap spawn?
        }
    }
}
