using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using OPJosMod.SupahNinja.Utils;
using UnityEngine;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(MouthDogAI))]
    internal class MouthDogAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("DetectNoise")]
        [HarmonyPrefix]
        private static bool detectNoisePatch(ref Vector3 noisePosition)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                var playerIndex = StartOfRound.Instance.localPlayerController.playerClientId;
                var currentPlayer = allPlayerScripts[playerIndex];

                if (GeneralUtils.twoPointsAreClose(noisePosition, currentPlayer.transform.position, 5f))
                {
                    mls.LogMessage("noise was close to you, so don't detect this noise");
                    return false;
                }
            }

            return true;
        }
    }
}
