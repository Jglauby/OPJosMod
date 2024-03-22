using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;
using OPJosMod.LagJutsu.Utils;
using Object = UnityEngine.Object;

namespace OPJosMod.LagJutsu.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static bool godMode = true;

        private static float lastToggledTime = Time.time;
        private static float toggleDelay = 0.2f;

        private static float lastTimeAddedLocation = Time.time;
        private static List<Vector3> lastSafeLocations = new List<Vector3>();

        private static float lastUpdatedKnownEnemies = Time.time;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void startPatch(PlayerControllerB __instance)
        {
            lastSafeLocations.Add(RoundManager.Instance.playersManager.playerSpawnPositions[0].transform.position);
            EnemyAIPatch.allEnemies = Object.FindObjectsOfType<EnemyAI>();
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.localPlayerController == null)
                return;

            if (__instance.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                handleGodModeToggle();
                updateKnownEnemies();

                //handle saving last locations you were safe at
                if (Time.time - lastTimeAddedLocation > 0.1 && __instance.thisController.isGrounded)
                {
                    //remove earliest in list if listsize is at max
                    if (lastSafeLocations.Count >= 600)
                        lastSafeLocations.RemoveAt(0);

                    //dont save the last safe location if it is basically the same spot as the last one that was saved
                    if (lastSafeLocations.Count > 0)
                    {
                        if (!GeneralUtil.AreVectorsClose(__instance.transform.position, lastSafeLocations[lastSafeLocations.Count - 1], 0.5f))
                        {
                            lastSafeLocations.Add(__instance.transform.position);
                            lastTimeAddedLocation = Time.time;
                        }
                    }
                    else
                    {
                        lastSafeLocations.Add(__instance.transform.position);
                        lastTimeAddedLocation = Time.time;
                    }
                }
            }
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static bool killPlayerPatch(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.localPlayerController == null || __instance.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                if (godMode)
                {
                    //mls.LogMessage("cant kill");
                    teleportPlayerBack();
                    return false;
                }
            }

            return true;
        }

        public static void teleportPlayerBack()
        {
            if (lastSafeLocations.Count > 0)
            {
                for (int i = lastSafeLocations.Count - 1; i >= 0; i--)
                {
                    var playerLocation = StartOfRound.Instance.localPlayerController.transform.position;
                    if (!GeneralUtil.ExistsCloseEnemy(lastSafeLocations[i]) && !GeneralUtil.AreVectorsClose(lastSafeLocations[i], playerLocation, 1f))
                    {
                        //lastSafeLocations.RemoveAt(i);
                        StartOfRound.Instance.localPlayerController.transform.position = lastSafeLocations[i];
                        mls.LogMessage($"teleport player to: {lastSafeLocations[i]}");

                        return;
                    }
                }
            }
        }

        private static void handleGodModeToggle()
        {
            if (((ButtonControl)Keyboard.current[ConfigVariables.DeathToggleButton]).wasPressedThisFrame)
            {
                if (Time.time - lastToggledTime > toggleDelay)
                {
                    string statusText = "";
                    switch (godMode)
                    {
                        case (true):
                            godMode = false;
                            statusText = "OFF";
                            break;
                        case (false):
                            godMode = true;
                            statusText = "ON";
                            break;
                    }
                    lastToggledTime = Time.time;
                    HUDManager.Instance.DisplayTip("God Mode", $"turned {statusText}");
                    mls.LogMessage($"god mode turned {statusText}");
                }
            }
        }

        private static void updateKnownEnemies()
        {
            if (Time.time - lastUpdatedKnownEnemies > 15)
            {
                EnemyAIPatch.allEnemies = Object.FindObjectsOfType<EnemyAI>();
                lastUpdatedKnownEnemies = Time.time;
            }
        }
    }
}
