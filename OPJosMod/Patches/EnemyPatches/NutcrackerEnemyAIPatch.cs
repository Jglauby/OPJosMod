using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }
        SupahNinja

        [HarmonyPatch("SwitchTargetToPlayer")]
        [HarmonyPrefix]
        private static bool switchTargetToPlayerPatch(NutcrackerEnemyAI __instance, ref int playerId)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if ((int)StartOfRound.Instance.localPlayerController.playerClientId == playerId)
                {
                    mls.LogMessage("kept nutcracker from switching target to current player");
                    __instance.StopInspection();
                    __instance.currentBehaviourStateIndex = 0;
                    return false;
                }
            }

            return true;
        }    
    }
}
