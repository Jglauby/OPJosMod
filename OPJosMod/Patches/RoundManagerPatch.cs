using BepInEx.Logging;
using HarmonyLib;

namespace OPJosMod.MoreEnemies.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        private static void loadNewLevelPatch(RoundManager __instance)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                mls.LogMessage("loggin enemies spawnrates");
                __instance.currentMaxInsidePower = __instance.currentMaxInsidePower * ConfigVariables.enemySpawnMultiplier;
                __instance.currentMaxOutsidePower = __instance.currentMaxOutsidePower * ConfigVariables.enemySpawnMultiplier;
                __instance.currentLevel.maxEnemyPowerCount = __instance.currentLevel.maxEnemyPowerCount * ConfigVariables.enemySpawnMultiplier;
                __instance.currentLevel.maxOutsideEnemyPowerCount = __instance.currentLevel.maxOutsideEnemyPowerCount * ConfigVariables.enemySpawnMultiplier;
            }
            else
            {
                mls.LogMessage("didn't adjust spawn rates as you arent host");
            }
        }
    }
}
