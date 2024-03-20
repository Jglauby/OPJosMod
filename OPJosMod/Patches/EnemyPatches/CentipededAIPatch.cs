using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(CentipedeAI))]
    internal class CentipedeAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool updatePrePatch(CentipedeAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                //do nothing if ghost is closest to centipede and its hanging from ceiling
                if (EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId
                    && __instance.currentBehaviourStateIndex == 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
