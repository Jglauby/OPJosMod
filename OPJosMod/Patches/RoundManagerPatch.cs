﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod;
using OPJosMod.HideNSeek.Config;
using OPJosMod.HideNSeek.Utils;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("FinishGeneratingNewLevelClientRpc")]
        [HarmonyPostfix]
        private static void finishGeneratingNewLevelClientRpcPatch(RoundManager __instance)
        {
            mls.LogMessage("FinishGeneratingNewLevelClientRpc patch hit");

            //if the round starts and you arent set as seeker, then you didnt pull the lever and you should be a hider
            if (PlayerControllerBPatch.isSeeker)
                PlayerControllerBPatch.SetupSeeker();
            else
                PlayerControllerBPatch.SetupHider();
        }

        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        private static void loadNewLevelPatch(RoundManager __instance)
        {
            mls.LogMessage("load new level patch hit");

            if (GameNetworkManager.Instance.isHostingGame)
            {
                //make no enemies spawn
                __instance.currentMaxInsidePower = 0;
                __instance.currentMaxOutsidePower = 0;
                __instance.currentLevel.maxEnemyPowerCount = 0;
                __instance.currentLevel.maxOutsideEnemyPowerCount = 0;

                //make no scrap spawn
                __instance.scrapAmountMultiplier = 0;

                //destroy all shovels
                //var shovels = Object.FindObjectsOfType<Shovel>();
                //for (int k = 0; k < shovels.Length; k++)
                //{
                //    Object.Destroy(shovels[k].gameObject);
                //}

                //create and drop shovel
                Terminal terminal = ReflectionUtils.GetFieldValue<Terminal>(HUDManager.Instance, "terminalScript");
                GameObject obj = Object.Instantiate(terminal.buyableItemsList[(int)BuyableItems.Shovel].spawnPrefab, __instance.playersManager.playerSpawnPositions[0].position, Quaternion.identity, StartOfRound.Instance.localPlayerController.playersManager.propsContainer);
                obj.GetComponent<GrabbableObject>().fallTime = 0f;
                obj.GetComponent<NetworkObject>().Spawn();

                //set time speed, more players => longer days
                var playerCount = __instance.playersManager.allPlayerScripts.Length;
                var daySpeedIncrease = 4f;
                TimeOfDay.Instance.globalTimeSpeedMultiplier = (daySpeedIncrease * 4) / playerCount;
            }
        }
    }
}
