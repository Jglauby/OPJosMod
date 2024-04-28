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
                //foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
                //{
                //    enemy.rarity = Mathf.Clamp(enemy.rarity * (int)Math.Round(ConfigVariables.enemySpawnMultiplier), 0, 100);
                //}

                //newLevel.enemySpawnChanceThroughoutDay = new AnimationCurve(
                //    new Keyframe(0f, 0.2f),
                //    new Keyframe(0.5f, 500f)
                //);
                //
                //newLevel.outsideEnemySpawnChanceThroughDay = new AnimationCurve(
                //    new Keyframe(0f, 20f),
                //    new Keyframe(1f, 50f)
                //);

                //inside
                if (ConfigVariables.spawnEnemiesInside)
                {
                    newLevel.enemySpawnChanceThroughoutDay = MultiplyAnimationCurve(__instance.currentLevel.enemySpawnChanceThroughoutDay, ConfigVariables.enemyInsideSpawnMultiplier);
                    __instance.currentMaxInsidePower = __instance.currentMaxInsidePower * ConfigVariables.enemyInsideSpawnMultiplier;
                    __instance.currentLevel.maxEnemyPowerCount = (int)Math.Round(__instance.currentLevel.maxEnemyPowerCount * ConfigVariables.enemyInsideSpawnMultiplier);
                }
                else
                {
                    __instance.currentMaxInsidePower = 0;
                }

                //outside
                if (ConfigVariables.spawnEnemiesOutside)
                {
                    newLevel.outsideEnemySpawnChanceThroughDay = MultiplyAnimationCurve(__instance.currentLevel.outsideEnemySpawnChanceThroughDay, ConfigVariables.enemyOutsideSpawnMultiplier, true);
                    newLevel.daytimeEnemySpawnChanceThroughDay = MultiplyAnimationCurve(__instance.currentLevel.daytimeEnemySpawnChanceThroughDay, ConfigVariables.enemyOutsideSpawnMultiplier, true);
                    __instance.currentMaxOutsidePower = __instance.currentMaxOutsidePower * ConfigVariables.enemyOutsideSpawnMultiplier;
                    __instance.currentLevel.maxOutsideEnemyPowerCount = (int)Math.Round(__instance.currentLevel.maxOutsideEnemyPowerCount * ConfigVariables.enemyOutsideSpawnMultiplier);
                }
                else
                {
                    __instance.currentMaxOutsidePower = 0;
                    __instance.currentLevel.maxOutsideEnemyPowerCount = 0;
                }
            }
            else
            {
                mls.LogMessage("didn't adjust spawn rates as you arent host");
            }
        }

        private static AnimationCurve MultiplyAnimationCurve(AnimationCurve animationCurve, float multiplicative, bool allowNegative = false)
        {
            AnimationCurve enemySpawnChanceThroughoutDay = animationCurve;
            AnimationCurve newCurve = new AnimationCurve();

            for (int i = 0; i < enemySpawnChanceThroughoutDay.length; i++)
            {
                Keyframe key = enemySpawnChanceThroughoutDay[i];
                float correctedValue = (allowNegative) ? Mathf.Max(key.value, 0.1f * (i + 1)) : key.value;

                //mls.LogMessage($"{i} Keyframe [{key.time}, {key.value}]");
                var newValue = (correctedValue > 0) ? correctedValue * multiplicative : correctedValue;
                Keyframe newKey = new Keyframe(key.time, newValue);

                newKey.inTangent = key.inTangent;
                newKey.outTangent = key.outTangent;

                newCurve.AddKey(newKey);
            }

            return newCurve;
        }
    }
}
