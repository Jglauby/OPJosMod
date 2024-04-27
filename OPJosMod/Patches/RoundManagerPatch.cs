using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.MoreEnemies.Utils;
using System;
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
                    enemy.rarity = Mathf.Clamp(enemy.rarity * (int)Math.Round(ConfigVariables.enemySpawnMultiplier), 0, 100);
                }

                //newLevel.enemySpawnChanceThroughoutDay = new AnimationCurve(
                //    new Keyframe(0f, 0.2f),
                //    new Keyframe(0.5f, 500f)
                //);
                //
                //newLevel.outsideEnemySpawnChanceThroughDay = new AnimationCurve(
                //    new Keyframe(0f, 20f),
                //    new Keyframe(1f, 50f)
                //);

                newLevel.enemySpawnChanceThroughoutDay = MultiplyAnimationCurve(__instance.currentLevel.enemySpawnChanceThroughoutDay, ConfigVariables.enemySpawnMultiplier);
                newLevel.outsideEnemySpawnChanceThroughDay = MultiplyAnimationCurve(__instance.currentLevel.outsideEnemySpawnChanceThroughDay, ConfigVariables.enemySpawnMultiplier);
                newLevel.daytimeEnemySpawnChanceThroughDay = MultiplyAnimationCurve(__instance.currentLevel.daytimeEnemySpawnChanceThroughDay, ConfigVariables.enemySpawnMultiplier);

                //__instance.currentLevel.spawnProbabilityRange = __instance.currentLevel.spawnProbabilityRange * ConfigVariables.enemySpawnMultiplier;
                __instance.currentMaxInsidePower = __instance.currentMaxInsidePower * ConfigVariables.enemySpawnMultiplier;
                __instance.currentMaxOutsidePower = __instance.currentMaxOutsidePower * ConfigVariables.enemySpawnMultiplier;
                __instance.currentLevel.maxEnemyPowerCount = (int)Math.Round(__instance.currentLevel.maxEnemyPowerCount * ConfigVariables.enemySpawnMultiplier);
                __instance.currentLevel.maxOutsideEnemyPowerCount = (int)Math.Round(__instance.currentLevel.maxOutsideEnemyPowerCount * ConfigVariables.enemySpawnMultiplier);
            }
            else
            {
                mls.LogMessage("didn't adjust spawn rates as you arent host");
            }
        }

        private static AnimationCurve MultiplyAnimationCurve(AnimationCurve animationCurve, float multiplicative)
        {
            AnimationCurve enemySpawnChanceThroughoutDay = animationCurve;
            AnimationCurve newCurve = new AnimationCurve();

            for (int i = 0; i < enemySpawnChanceThroughoutDay.length; i++)
            {
                Keyframe key = enemySpawnChanceThroughoutDay[i];

                Keyframe newKey = new Keyframe(key.time, key.value * ConfigVariables.enemySpawnMultiplier);

                newKey.inTangent = key.inTangent;
                newKey.outTangent = key.outTangent;

                newCurve.AddKey(newKey);
            }

            return newCurve;
        }
    }
}
