using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.MoreEnemies.Utils;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
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
        private static void loadNewLevelPatch(RoundManager __instance, ref SelectableLevel newLevel)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                mls.LogMessage("adjusting enemies spawnrates");
                foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
                {
                    enemy.rarity = Mathf.Clamp(enemy.rarity * ConfigVariables.enemySpawnMultiplier, 0, 100);
                }

                newLevel.enemySpawnChanceThroughoutDay = new AnimationCurve(
                    new Keyframe(0f, 0.2f),
                    new Keyframe(0.5f, 500f)
                );

                newLevel.outsideEnemySpawnChanceThroughDay = new AnimationCurve(
                    new Keyframe(0f, 20f),
                    new Keyframe(1f, 50f)
                );

                //__instance.currentLevel.spawnProbabilityRange = __instance.currentLevel.spawnProbabilityRange * ConfigVariables.enemySpawnMultiplier;
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
