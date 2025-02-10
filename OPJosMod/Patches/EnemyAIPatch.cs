using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.Utils;
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

namespace OPJosMod.OPClientSide.Patches
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
                    //mls.LogMessage("enemy collide with player patch hit");
                    return false;
                }
            }
        
            return true;
        }

        [HarmonyPatch("KillEnemy")]
        [HarmonyPrefix]
        static bool killEnemyPatch(EnemyAI __instance)
        {
            return stopKill(__instance);
        }

        [HarmonyPatch("KillEnemyOnOwnerClient")]
        [HarmonyPrefix]
        static bool patchKillEnemyOnOwnerClient(EnemyAI __instance)
        {
            return stopKill(__instance);
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
                //mls.LogMessage("Closest player found: " + resultingPlayer.name);
            }
            else
            {
                mls.LogError("No EnemyAI found in the scene.");
            }

            return resultingPlayer;
        }

        public static void makeEnemiesDropFocus(PlayerControllerB player)
        {
            mls.LogMessage($"enemies dropped focus on {player.name}");

            EnemyAI[] allEnemies = Object.FindObjectsOfType<EnemyAI>();
            foreach(EnemyAI enemy in allEnemies)
            {
                if (enemy.targetPlayer.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                    enemy.targetPlayer = null;
                
                //if (enemy is CrawlerAI)
                //{
                //    mls.LogMessage("enemy is a crawler");
                //    CrawlerAI crawlerEnemy = (CrawlerAI)enemy;
                //
                //    ReflectionUtils.SetFieldValue(crawlerEnemy, "hasEnteredChaseMode", false);
                //    crawlerEnemy.SwitchToBehaviourStateOnLocalClient(0);
                //}
            }
        }

        public static bool ghostOnlyPlayerInFacility()
        {
            int countInFactory = 0; //players in factory that arent ghost
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled && StartOfRound.Instance.allPlayerScripts[i].isInsideFactory
                    && StartOfRound.Instance.allPlayerScripts[i].playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
                {
                    countInFactory++;
                }
            }

            if (countInFactory == 0)
            {
                mls.LogMessage("ghost only one in facility");
                return true;
            }

            return false;
        }

        private static bool stopKill(EnemyAI enemy)
        {
            mls.LogMessage("enemy ai tried hit killEnemyPatch");
            if (PlayerControllerBPatch.isGhostMode && enemy.GetClosestPlayer().playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                mls.LogMessage("enemy ai tried to direclty kill player");
                return false;
            }

            return true;
        }
    }
}
