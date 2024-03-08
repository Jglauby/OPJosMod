using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GhostMode.Patches;

namespace OPJosMod.GhostMode.Enemy.Patches
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
    }
}
