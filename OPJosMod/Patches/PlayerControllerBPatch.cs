using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.BreadCrumbs.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

namespace OPJosMod.BreadCrumbs.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch : MonoBehaviour
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static bool togglingAutoMove = false;
              
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (__instance.IsOwner && !__instance.inTerminalMenu && !__instance.isTypingChat && __instance.isInsideFactory)//can toggle
            {
                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.retraceButton]).wasPressedThisFrame)//H was pressed, default
                    {
                        if (togglingAutoMove == false)
                        {
                            togglingAutoMove = true;
                            __instance.StartCoroutine(toggleSpeed(__instance));
                        }
                    }
                }
                catch { }
            }

            AutoWalk(__instance);

            //prevents manual camera movement while auto walking is on
            //if (((Behaviour)(object)agent).enabled == true)
            //{
            //    __instance.playerActions.Movement.Disable();
            //}
            //else
            //{
            //    __instance.playerActions.Movement.Enable();
            //}
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        private static void killPlayerPatch(PlayerControllerB __instance)
        {
            if (__instance.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                RemoveMeshForPlayer();
            }
        }

        private static IEnumerator toggleSpeed(PlayerControllerB __instance)
        {
            yield return new WaitForSeconds(0.2f);

            if (moveTowardsDestination == false)
            {
                HUDManager.Instance.DisplayTip("Headed Back", "Started");
                moveTowardsDestination = true;
                InitializeNaveMeshForPlayer();

                yield return new WaitForSeconds(0.2f);
                startRunToNewPosition(__instance);
            }
            else if (moveTowardsDestination == true)
            {
                HUDManager.Instance.DisplayTip("Headed Back", "Stopped");
                ((Behaviour)(object)agent).enabled = false;
                moveTowardsDestination = false;

                yield return new WaitForSeconds(0.2f);
                RemoveMeshForPlayer();
            }

            togglingAutoMove = false;
        }

        private static NavMeshPath path1;
        private static NavMeshAgent agent;
        private static bool moveTowardsDestination = false;
        private static Vector3 destination;
        public static bool hasInitialized = false;       
        public static float lastClickedAt = Time.time;
        public static float clickDelay = 0.2f;

        private static void AutoWalk(PlayerControllerB __instance)
        {
            if (hasInitialized && __instance.gameObject.GetComponent<NavMeshAgent>() != null)
            {
                if (moveTowardsDestination && __instance.isInsideFactory)
                {
                    if (Vector3.Distance(__instance.transform.position, destination) < 1)
                    {
                        mls.LogMessage("reached destination!");
                        HUDManager.Instance.DisplayTip("Arrived!", "");

                        ((Behaviour)(object)agent).enabled = false;
                        moveTowardsDestination = false;
                    } 

                    agent.SetDestination(destination);
                }
            }
        }

        public static void InitializeNaveMeshForPlayer()
        {
            try
            {
                var player = StartOfRound.Instance.localPlayerController;
                if (player == null || player.gameObject == null || player.isInsideFactory == false)
                {
                    mls.LogMessage("PlayerControllerB instance or its GameObject is null. or they are outside");
                    return;
                }
                //mls.LogMessage("Initializing NavMeshAgent for player with client ID: " + player.playerClientId);

                if (player.gameObject.GetComponent<NavMeshAgent>() == null)
                {

                    agent = player.gameObject.AddComponent<NavMeshAgent>();

                    agent.speed = 2f;
                    agent.acceleration = 25f;
                    agent.angularSpeed = 225f;
                    agent.stoppingDistance = 0.5f; 
                    agent.autoBraking = false;
                    agent.autoTraverseOffMeshLink = false; 
                    agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance; 
                    agent.radius = 0.4f; 
                    agent.height = 2.0f; 
                    agent.avoidancePriority = 99;
                    agent.autoRepath = true;

                    hasInitialized = true;
                }
                else
                {
                    agent = player.gameObject.GetComponent<NavMeshAgent>();
                    //mls.LogMessage("didnt re-add nav mesh as it already existed");
                }

                //start with it off
                ((Behaviour)(object)agent).enabled = false;
                moveTowardsDestination = false;
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }

        public static void RemoveMeshForPlayer()
        {
            var player = StartOfRound.Instance.localPlayerController;
            if (player.gameObject.GetComponent<NavMeshAgent>() != null)
            {
                Destroy(player.gameObject.GetComponent<NavMeshAgent>());
            }
        }

        private static void startRunToNewPosition(PlayerControllerB player)
        {
            ((Behaviour)(object)agent).enabled = true;

            //set destination to the front door
            if (SetDestinationToPosition(RoundManager.FindMainEntrancePosition()) == false)
            {
                //failed first time, try once more
                SetDestinationToPosition(RoundManager.FindMainEntrancePosition());
            }
        }

        private static bool SetDestinationToPosition(Vector3 position)
        {
            try
            {
                //mls.LogMessage($"setting desitination to positon: {position}");
                position = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 1.75f);

                moveTowardsDestination = true;
                destination = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, -1f);
                //mls.LogMessage($"destination is: {destination}");
                return true;
            }
            catch (Exception e)
            {
                mls.LogError(e);
                InitializeNaveMeshForPlayer();
            }

            return false;
        }
    }
}
