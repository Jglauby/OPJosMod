using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(FlowermanAI))]
    internal class FlowermanAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("AvoidClosestPlayer")]
        [HarmonyPrefix]
        private static bool avoidClosestPlayerPatch(FlowermanAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if (EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch("AddToAngerMeter")]
        [HarmonyPrefix]
        private static bool addToAngerMeterPatch(FlowermanAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                bool hasPlayerInside = false;
                foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (player.isInsideFactory && player.playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
                    {
                        hasPlayerInside = true;
                        break;
                    }
                }

                return hasPlayerInside;
            }

            return true; 
        }
    }
}
