using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.SupahNinja.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OPJosMod.SupahNinja.Patches
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
            if (GeneralUtils.playerIsCrouching())
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

        [HarmonyPatch("PlayerIsTargetable")]
        [HarmonyPrefix]
        static void playerIsTargetablePatch(ref bool cannotBeInShip, ref PlayerControllerB playerScript)
        {
            //a way to make current player always return not tragetable in the enemy ai
            if (GeneralUtils.playerIsCrouching())
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
            if (GeneralUtils.playerIsCrouching())
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
            if (GeneralUtils.playerIsCrouching() && __instance.GetClosestPlayer().playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                mls.LogMessage("enemy ai tried to direclty kill player");
                return false;
            }

            return true;
        }

        [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
        [HarmonyPrefix]
        static bool checkLineOfSightForClosestPlayerPatch(EnemyAI __instance)
        {
            if (GeneralUtils.playerIsCrouching())
            {
                if (EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                {
                    mls.LogMessage("enemy can't see if player is in line of sight");
                    return false;
                }
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
    }
}
