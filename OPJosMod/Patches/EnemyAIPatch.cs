﻿using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace OPJosMod.GhostMode.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool onCollideWithPlayerPatch(EnemyAI __instance, ref Collider other)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();

                if (StartOfRound.Instance.localPlayerController.playerClientId == component.playerClientId)
                {
                    mls.LogMessage("enemy collide with player patch hit");
                    return false;
                }
            }
        
            return true;
        }

        [HarmonyPatch("PlayerIsTargetable")]
        [HarmonyPrefix]
        static void playerIsTargetablePatch(ref bool cannotBeInShip, ref PlayerControllerB playerScript)
        {
            //a way to make current player always return not tragetable in the enemy ai
            if (PlayerControllerBPatch.isGhostMode && playerScript.IsOwner)
            {
                //mls.LogMessage("set local player to not targetable");
                playerScript.isInHangarShipRoom = true;
                cannotBeInShip = true;
            }
        }

        [HarmonyPatch("GetAllPlayersInLineOfSight")]
        [HarmonyPostfix]
        static void getAllPlayersInLineOfSightPatch(EnemyAI __instance, ref PlayerControllerB[] __result)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                var playerIndex = StartOfRound.Instance.localPlayerController.playerClientId;

                if (__result != null && __result.Length > 0)
                {
                    // Find the index of the current player in the result array
                    int currentPlayerIndex = -1;
                    for (int i = 0; i < __result.Length; i++)
                    {
                        if (__result[i] == allPlayerScripts[playerIndex])
                        {
                            currentPlayerIndex = i;
                            break;
                        }
                    }

                    // If the current player is found in the result array, remove it
                    if (currentPlayerIndex != -1)
                    {
                        // Create a new array to store the modified result
                        PlayerControllerB[] modifiedResult = new PlayerControllerB[__result.Length - 1];
                        int newIndex = 0;
                        for (int i = 0; i < __result.Length; i++)
                        {
                            if (i != currentPlayerIndex)
                            {
                                modifiedResult[newIndex] = __result[i];
                                newIndex++;
                            }
                        }

                        // Assign the modified result back to the __result parameter
                        __result = modifiedResult;
                    }
                }
            }
        }

        [HarmonyPatch("KillEnemy")]
        [HarmonyPrefix]
        static bool killEnemyPatch(EnemyAI __instance)
        {
            mls.LogMessage("enemy ai tried hit killEnemyPatch");
            if (PlayerControllerBPatch.isGhostMode && __instance.GetClosestPlayer().playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                mls.LogMessage("enemy ai tried to direclty kill player");
                return false;
            }

            return true;
        }

        public static PlayerControllerB getClosestPlayerIncludingGhost(EnemyAI enemy)
        {
            PlayerControllerB resultingPlayer = null;

            PlayerControllerB[] players = Object.FindObjectsOfType<PlayerControllerB>();
            float closestDistance = Mathf.Infinity;

            foreach (PlayerControllerB player in players)
            {
                float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    resultingPlayer = player;
                }
            }

            if (resultingPlayer != null)
            {
                mls.LogMessage("Closest player found: " + resultingPlayer.name);
            }
            else
            {
                mls.LogError("No EnemyAI found in the scene.");
            }

            return resultingPlayer;
        }
    }
}
