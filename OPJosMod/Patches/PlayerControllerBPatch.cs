using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod;
using OPJosMod.HideNSeek.Config;
using OPJosMod.HideNSeek.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static bool hasSetRole = false;
        public static bool isSeeker = false;
        public static bool isHider = false;

        private static float seekerDelay = 15f;

        private static Coroutine teleportCoroutine;
        private static Coroutine lockPlayerCoroutine;

        private static float sprintMultiplier = 1.01f;
        private static float maxSprintSpeed = 3f;

        private static float lastCheckedTime;
        private static float checkGameOverFrequency = 5;

        public static void resetRoleValues()
        {
            hasSetRole = false;
            isSeeker = false;
            isHider = false;
            lastCheckedTime = Time.time;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void updatePatch(PlayerControllerB __instance)
        {
            if (isSeeker)
            {
                var isWalking = ReflectionUtils.GetFieldValue<bool>(__instance, "isWalking");
                if (isWalking)
                {
                    var currentValue = ReflectionUtils.GetFieldValue<float>(__instance, "sprintMultiplier");
                    if (__instance.isSprinting)
                    {
                        var newForce = (float)currentValue * sprintMultiplier;

                        if (newForce < maxSprintSpeed)
                            ReflectionUtils.SetFieldValue(__instance, "sprintMultiplier", newForce);
                    }
                }
            }

            if (Time.time - lastCheckedTime > checkGameOverFrequency)
            {
                checkIfShouldEndRound(__instance);
                lastCheckedTime = Time.time;
            }
        }

        private static void checkIfShouldEndRound(PlayerControllerB __instance)
        {
            var totalPlayerCount = RoundManager.Instance.playersManager.allPlayerScripts.Where(x => x.isPlayerControlled).Count();
            var shipLocation = RoundManager.Instance.playersManager.playerSpawnPositions[0].position;

            //one person alive
            if (totalPlayerCount == 1)
            {
                mls.LogMessage($"one person alive, round over. totalPlayers:{totalPlayerCount}");
                StartOfRound.Instance.EndGameServerRpc((int)__instance.playerClientId);
            }
        }

        public static void SetupHider()
        {
            if (hasSetRole)
                return;

            hasSetRole = true;
            mls.LogMessage("setup player as a hider");
            PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
            isSeeker = false;
            isHider = true;

            //teleport player inside
            teleportCoroutine = localPlayerController.StartCoroutine(customTeleportPlayer(localPlayerController, RoundManager.FindMainEntrancePosition(), 5f));
            localPlayerController.isInsideFactory = true;
        }

        public static void SetupSeeker()
        {
            if (hasSetRole)
                return;

            hasSetRole = true;
            mls.LogMessage("setup player as seeker");
            PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
            isSeeker = true;
            isHider = false;

            lockPlayerCoroutine = localPlayerController.StartCoroutine(lockPlayer(localPlayerController, seekerDelay));

            //force enemies to whistle, on cool down
                //will need some sort of existing server function i can call so that the noise is heard across all clients not just seekers client
        }

        private static IEnumerator customTeleportPlayer(PlayerControllerB player, Vector3 location, float initalDelay)
        {
            if (teleportCoroutine != null)
            {
                player.StopCoroutine(teleportCoroutine);
            }

            yield return new WaitForSeconds(initalDelay);

            try
            {
                for (int i = 0; i < player.ItemSlots.Length; i++)
                {
                    player.DestroyItemInSlotAndSync(i);
                }
            }
            catch { }

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(location);
        }

        private static IEnumerator lockPlayer(PlayerControllerB player, float lockTime)
        {
            if (lockPlayerCoroutine != null)
            {
                player.StopCoroutine(lockPlayerCoroutine);
            }

            yield return new WaitForSeconds(1f);
            mls.LogMessage("player locked in place");
            player.playerCollider.enabled = false;

            //add some sort of UI to indicate you are waiting and for how long
            //will need to loop with smaller delays to update the UI every second

            yield return new WaitForSeconds(lockTime);
            mls.LogMessage("player unlocked!");
            player.playerCollider.enabled = true;
        }
    }
}
