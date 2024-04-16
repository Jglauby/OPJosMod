using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.GhostMode.Patches;
using OPJosMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
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
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

namespace OPJosMod.GhostMode.Enemy.Patches
{
    [HarmonyPatch(typeof(ButlerEnemyAI))]
    internal class ButlerEnemyAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool onCollideWithPlayerPatch(ref Collider other)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                if (StartOfRound.Instance.localPlayerController.playerClientId == component.playerClientId)
                {
                    //mls.LogMessage("mech collide with player patch hit");
                    return false;
                }
            }

            return true;
        }

        private static bool dontCallForgetPlayers = false;
        [HarmonyPatch("CheckLOS")]
        [HarmonyPrefix]
        static bool patchCheckLOS(ButlerEnemyAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                bool[] seenPlayers = ReflectionUtils.GetFieldValue<bool[]>(__instance, "seenPlayers");
                Vector3[] lastSeenPlayerPositions = ReflectionUtils.GetFieldValue<Vector3[]>(__instance, "lastSeenPlayerPositions");
                float[] timeOfLastSeenPlayers = ReflectionUtils.GetFieldValue<float[]>(__instance, "timeOfLastSeenPlayers");
                PlayerControllerB watchingPlayer = ReflectionUtils.GetFieldValue<PlayerControllerB>(__instance, "watchingPlayer");

                int num = 0;
                float num2 = 10000f;
                int num3 = -1;
                for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    if (StartOfRound.Instance.allPlayerScripts[i].isPlayerDead || !StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled)
                    {
                        seenPlayers[i] = false;
                        continue;
                    }

                    if (__instance.CheckLineOfSightForPosition(StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position, 110f, 60, 2f))
                    {
                        num++;
                        lastSeenPlayerPositions[i] = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position;
                        seenPlayers[i] = true;
                        timeOfLastSeenPlayers[i] = Time.realtimeSinceStartup;
                    }
                    else if (seenPlayers[i])
                    {
                        num++;
                    }

                    if (seenPlayers[i])
                    {
                        float num4 = Vector3.Distance(__instance.eye.position, StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position);
                        if (num4 < num2)
                        {
                            num2 = num4;
                            num3 = i;
                        }
                    }
                }

                if (__instance.currentBehaviourStateIndex == 2)
                {
                    return true;
                }

                if (num3 != -1)
                {
                    watchingPlayer = StartOfRound.Instance.allPlayerScripts[num3];
                    if (__instance.currentBehaviourStateIndex != 2)
                    {
                        //__instance.targetPlayer = watchingPlayer;
                        if (watchingPlayer != null && watchingPlayer.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                        {
                            //mls.LogMessage("dont target player");
                            dontCallForgetPlayers = true;
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        //this is called right after the checkLOS funciton, so if that isnt called we need to make sure this doestn call
        [HarmonyPatch("ForgetSeenPlayers")]
        [HarmonyPrefix]
        static bool patchForgetSeenPlayers(ButlerEnemyAI __instance)
        {
            if (dontCallForgetPlayers)
                return false;

            return true;
        }

        //the loc and forget seeen players is called in the update, so after each update u need to ensure that forgetSeeenPlayers can be called again
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(ButlerEnemyAI __instance)
        {
            dontCallForgetPlayers = false;
        }
    }
}
