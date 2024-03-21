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

        private static float lastTimeDied = Time.time;
        private static float lastTimeAddedLocation = Time.time;
        private static List<Vector3> lastSafeLocations = new List<Vector3>();

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void startPatch(PlayerControllerB __instance)
        {
            lastSafeLocations.Add(RoundManager.Instance.playersManager.playerSpawnPositions[0].transform.position);
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (StartOfRound.Instance.localPlayerController == null || __instance.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                handleGodModeToggle();

                //handle saving last locations you were safe at
                if (Time.time - lastTimeDied > 1 && Time.time - lastTimeAddedLocation > 1 && __instance.thisController.isGrounded)
                {
                    //mls.LogMessage($"time.time:{Time.time} lastTimeDied:{lastTimeDied} rwindBackTimeSeconds:{ConfigVariables.RewindBackTimeSeconds}");
                    lastTimeAddedLocation = Time.time;

                    //remove earliest in list if listsize is at max
                    if (lastSafeLocations.Count >= ConfigVariables.RewindBackTimeSeconds * 10)
                        lastSafeLocations.RemoveAt(0);

                    //dont save the last safe location if it is basically the same spot as the last one that was saved
                    if (lastSafeLocations.Count > 0)
                    {
                        if (!GeneralUtil.AreVectorsClose(__instance.transform.position, lastSafeLocations[lastSafeLocations.Count - 1], 0.1f))
                            lastSafeLocations.Add(__instance.transform.position);
                    }
                    else
                        lastSafeLocations.Add(__instance.transform.position);
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
                    lastTimeDied = Time.time;
                    return false;
                }
            }

            return true;
        }

        public static void teleportPlayerBack()
        {
            if (lastSafeLocations.Count > 0)
            {
                Vector3 newLocation = new Vector3();
                if (lastSafeLocations.Count > ConfigVariables.RewindBackTimeSeconds)
                {
                    newLocation = lastSafeLocations[lastSafeLocations.Count - ConfigVariables.RewindBackTimeSeconds];
                    lastSafeLocations.RemoveAt(lastSafeLocations.Count - ConfigVariables.RewindBackTimeSeconds);
                }
                else if (lastSafeLocations.Count > 0)
                {
                    newLocation = lastSafeLocations[lastSafeLocations.Count - 1];
                    lastSafeLocations.RemoveAt(lastSafeLocations.Count - 1);
                }
                else
                {
                    newLocation = lastSafeLocations[0];
                }

                StartOfRound.Instance.localPlayerController.transform.position = newLocation;
                mls.LogMessage($"teleport player to: {newLocation}");
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
    }
}
