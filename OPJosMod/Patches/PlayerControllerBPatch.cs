using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod;
using OPJosMod.HideNSeek.Config;
using OPJosMod.HideNSeek.CustomRpc;
using OPJosMod.HideNSeek.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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

        private static Coroutine teleportCoroutine;
        private static Coroutine lockPlayerCoroutine;

        private static float lastCheckedTime;
        private static float checkGameOverFrequency = 5;

        private static float lastUsedSeekerAbilityAt = Time.time;

        private static int lastCheckedAliveCount = -1;

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
                        var newForce = (float)currentValue * ConfigVariables.seekerSprintMultiplier;

                        if (newForce < ConfigVariables.seekerMaxSprintSpeed)
                            ReflectionUtils.SetFieldValue(__instance, "sprintMultiplier", newForce);
                    }
                }

                if (Mouse.current.rightButton.wasPressedThisFrame)//right click was clicked
                {                    
                    if (Time.time - lastUsedSeekerAbilityAt > ConfigVariables.seekerAbilityCD)
                    {
                        lastUsedSeekerAbilityAt = Time.time;
                        makeClosestPlayerWhistle(StartOfRound.Instance.localPlayerController);
                    }
                    else
                    {
                        //dont display if just clicked
                        if (Time.time - lastUsedSeekerAbilityAt > 1)
                            HUDManagerPatch.CustomDisplayTip("Ability on cooldown", $"for {(int)(ConfigVariables.seekerAbilityCD - (Time.time - lastUsedSeekerAbilityAt))} seconds", false);
                    }
                }
            }

            if (Time.time - lastCheckedTime > checkGameOverFrequency)
            {
                checkIfShouldEndRound(__instance);
                lastCheckedTime = Time.time;
            }

            //round is actually happening
            if (isSeeker || isHider)
            {
                HUDManager.Instance.SetClockVisible(true);
            }
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void startPatch(PlayerControllerB __instance)
        {
            __instance.allHelmetLights[(int)FlashlightTypes.NormalFlashlight].intensity = __instance.allHelmetLights[(int)FlashlightTypes.NormalFlashlight].intensity / ConfigVariables.smallFlashlightPower;
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPostfix]
        private static void killPlayerPatch(PlayerControllerB __instance)
        {
            if (isSeeker)
            {
                mls.LogMessage("end game called as you are a seeker");
                StartOfRound.Instance.EndGameServerRpc((int)__instance.playerClientId);
            }
        }

        private static void checkIfShouldEndRound(PlayerControllerB __instance)
        {
            var totalPlayerCount = RoundManager.Instance.playersManager.allPlayerScripts.Where(x => x.isPlayerControlled).Count();
            var shipLocation = RoundManager.Instance.playersManager.playerSpawnPositions[0].position;

            if (totalPlayerCount < lastCheckedAliveCount && lastCheckedAliveCount != -1)
            {
                if (isSeeker)
                    HUDManagerPatch.CustomDisplayTip("Someone has Died!", $"{totalPlayerCount} players remain", false);
                else if (isHider)
                    HUDManagerPatch.CustomDisplayBigMessage($"Someone Died! {totalPlayerCount} players remain");
            }
            lastCheckedAliveCount = totalPlayerCount;

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

            HUDManager.Instance.DisplayTip("Role Set", "You are a Hider!");

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

            HUDManager.Instance.DisplayTip("Role Set", "You are Seeker!");

            hasSetRole = true;
            mls.LogMessage("setup player as seeker");
            PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
            isSeeker = true;
            isHider = false;

            lockPlayerCoroutine = localPlayerController.StartCoroutine(lockPlayer(localPlayerController, ConfigVariables.seekerDelay));

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

            player.DropAllHeldItemsAndSync();

            yield return new WaitForSeconds(1f);

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(location);
            //GameNetworkManager.Instance.localPlayerController.isInElevator = false;
            //GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
            //GameNetworkManager.Instance.localPlayerController.isInsideFactory = true;
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

            for (int i = 0; i < lockTime; i++)
            {
                yield return new WaitForSeconds(1f);
                HUDManagerPatch.CustomDisplayTip($"{lockTime - (i + 1)}", "seconds remain", false);
            }
            HUDManagerPatch.CustomDisplayTip($"GO FIND", "THEM!", false);

            mls.LogMessage("player unlocked!");
            player.playerCollider.enabled = true;

            //let hiders know u lookin for them
            string message = MessageTaskUtil.GetCode(MessageTasks.StartedSeeking);
            RpcMessage rpcMessage = new RpcMessage(message, (int)player.playerClientId, MessageCodes.Request);
            RpcMessageHandler.SendRpcMessage(rpcMessage);
        }

        private static void makeClosestPlayerWhistle(PlayerControllerB localPlayer)
        {
            mls.LogMessage("Making closest player whistle");
            HUDManagerPatch.CustomDisplayTip("Closest Player", "made a noise", false);
            var closestPlayer = findClosestPlayer(localPlayer);
            PlaySounds.PlayFart(closestPlayer);

            //send message
            string message = MessageTaskUtil.GetCode(MessageTasks.MakePlayerWhistle) + closestPlayer.playerClientId;
            RpcMessage rpcMessage = new RpcMessage(message, (int)localPlayer.playerClientId, MessageCodes.Request);
            RpcMessageHandler.SendRpcMessage(rpcMessage);
        }

        private static PlayerControllerB findClosestPlayer(PlayerControllerB localPlayer)
        {
            PlayerControllerB closestPlayer = null;
            float currentClosestDistance = float.MaxValue;

            var allPlayers = RoundManager.Instance.playersManager.allPlayerScripts;
            foreach (PlayerControllerB player in allPlayers)
            {
                if (player.playerClientId != localPlayer.playerClientId)
                {
                    float distance = Vector3.Distance(localPlayer.transform.position, player.transform.position);

                    if (distance < currentClosestDistance)
                    {
                        closestPlayer = player;
                        currentClosestDistance = distance;
                    }
                }
            }

            return closestPlayer;
        }
    }
}
