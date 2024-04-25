using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.MoreEnemies.Utils;
using System.Collections.Generic;
using UnityEngine;

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
                mls.LogMessage("adjusting enemies spawnrates");
                //__instance.hourTimeBetweenEnemySpawnBatches = __instance.hourTimeBetweenEnemySpawnBatches / ConfigVariables.enemySpawnMultiplier;
                __instance.currentLevel.spawnProbabilityRange = __instance.currentLevel.spawnProbabilityRange * ConfigVariables.enemySpawnMultiplier;
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

        [HarmonyPatch("AssignRandomEnemyToVent")]
        [HarmonyPrefix]
        private static void patchAssignRandomEnemyToVent(RoundManager __instance, ref List<int> SpawnProbabilities)
        {
            for (int i = 0; i < SpawnProbabilities.Count; i++)
            {
                SpawnProbabilities[i] *= ConfigVariables.enemySpawnMultiplier; 
            }
        }

        [HarmonyPatch("SpawnRandomOutsideEnemy")]
        [HarmonyPrefix]
        private static void patchSpawnRandomOutsideEnemy(RoundManager __instance, ref List<int> SpawnProbabilities)
        {
            for (int i = 0; i < SpawnProbabilities.Count; i++)
            {
                SpawnProbabilities[i] *= ConfigVariables.enemySpawnMultiplier; 
            }
        }
    }
}
