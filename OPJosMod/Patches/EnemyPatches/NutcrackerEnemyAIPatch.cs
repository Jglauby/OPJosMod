using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using OPJosMod.SupahNinja.Utils;

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

        [HarmonyPatch("SwitchTargetToPlayer")]
        [HarmonyPrefix]
        private static bool switchTargetToPlayerPatch(NutcrackerEnemyAI __instance, ref int playerId)
        {
            if (GeneralUtils.playerIsCrouching())
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
