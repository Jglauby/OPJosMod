using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.TheFlash.Utils;
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

namespace OPJosMod.TheFlash.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch : MonoBehaviour
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;

            sprintMultiplier = ConfigVariables.defaultSprintMultiplier;
            maxSprintSpeed = ConfigVariables.defaultMaxSprintSpeed;
        }
              
        private static float increasedSprintMultiplier = 10f;
        private static float increasedMaxSprintSpeed = 1500f;
        private static float sprintMultiplier;
        private static float maxSprintSpeed;

        private static bool adjustingSpeed = false;
        private static int speedMode = 0; //0 -> default, 1 -> super fast

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {            
            FieldInfo sprintMultiplierField = typeof(PlayerControllerB).GetField("sprintMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            var isWalking = ReflectionUtils.GetFieldValue<bool>(__instance, "isWalking");

            if (isWalking)
            {
                var currentValue = sprintMultiplierField.GetValue(__instance);
                if (__instance.isSprinting)
                {
                    var newForce = (float)currentValue * sprintMultiplier;

                    if(newForce < maxSprintSpeed)
                        sprintMultiplierField.SetValue(__instance, newForce);
                }
                else
                {
                    if ((float)currentValue > ConfigVariables.maxWalkSpeed)
                        sprintMultiplierField.SetValue(__instance, ConfigVariables.maxWalkSpeed);

                    var newForce = (float)currentValue * ConfigVariables.walkMultiplier;

                    if(newForce < ConfigVariables.maxWalkSpeed)
                        sprintMultiplierField.SetValue(__instance, newForce);
                }
            }

            if (__instance.IsOwner && !__instance.inTerminalMenu && !__instance.isTypingChat)//can toggle
            {
                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.flashTimeButton]).wasPressedThisFrame)//R was pressed, default
                    {
                        if (adjustingSpeed == false)
                        {
                            adjustingSpeed = true;
                            __instance.StartCoroutine(toggleSpeed(__instance));
                        }
                    }
                } catch { }
            }

            AutoWalk(__instance);

            //vibrate player
            //__instance.StartCoroutine(vibratePlayer(__instance));
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        private static void killPlayerPatch(PlayerControllerB __instance)
        {
            if (__instance.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                if (__instance.gameObject.GetComponent<NavMeshAgent>() != null)
                {
                    Destroy(__instance.gameObject.GetComponent<NavMeshAgent>());
                }
            }
        }

        private static IEnumerator toggleSpeed(PlayerControllerB __instance)
        {
            yield return new WaitForSeconds(1f);

            if (speedMode == 0)
            {
                mls.LogMessage("speed mode set to 1");
                HUDManager.Instance.DisplayTip("Flash Time", "On");

                speedMode = 1;
                sprintMultiplier = increasedSprintMultiplier;
                maxSprintSpeed = increasedMaxSprintSpeed;
            }
            else if (speedMode == 1)
            {
                mls.LogMessage("speed mode set to 0");
                HUDManager.Instance.DisplayTip("Flash Time", "Off");

                speedMode = 0;
                sprintMultiplier = ConfigVariables.defaultSprintMultiplier;
                maxSprintSpeed = ConfigVariables.defaultMaxSprintSpeed;
            }

            adjustingSpeed = false;
        }

        private static IEnumerator vibratePlayer(PlayerControllerB __instance)
        {
            float vibrateAmount = 0.005f;

            Vector3 forwardDirection = __instance.transform.forward;
            Vector3 rightDirection = Vector3.Cross(Vector3.up, forwardDirection);
            float randomDirectionSign = Random.Range(0, 2) == 0 ? -1f : 1f;
            Vector3 randomVibration = vibrateAmount * randomDirectionSign * rightDirection;

            __instance.thisController.Move(randomVibration);
            yield return new WaitForSeconds(0.005f);
            __instance.thisController.Move(-randomVibration);
            yield return new WaitForSeconds(0.005f);
        }




        #region smartMovement

        private static NavMeshPath path1;
        private static NavMeshAgent agent;
        private static bool moveTowardsDestination = false;
        private static Vector3 destination;
        public static bool hasInitialized = false;
        private static List<Vector3> runToLocations = null;

        private static void AutoWalk(PlayerControllerB __instance)
        {
            if (hasInitialized)
            {
                if (((ButtonControl)Keyboard.current[Key.B]).wasPressedThisFrame)
                {
                    startRunToNewPosition(__instance);                   
                }

                if (((ButtonControl)Keyboard.current[Key.C]).wasPressedThisFrame)
                {
                    ((Behaviour)(object)agent).enabled = false;
                    moveTowardsDestination = false;
                }

                if (moveTowardsDestination)
                {
                    if (Vector3.Distance(__instance.transform.position, destination) < 3)
                    {
                        mls.LogMessage("reached destination!");
                        startRunToNewPosition(__instance);
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
                if (player == null || player.gameObject == null)
                {
                    Debug.LogError("PlayerControllerB instance or its GameObject is null.");
                    return;
                }
                Debug.Log("Initializing NavMeshAgent for player with client ID: " + player.playerClientId);

                if (player.gameObject.GetComponent<NavMeshAgent>() == null)
                {

                    int multiplier = 1000;
                    agent = player.gameObject.AddComponent<NavMeshAgent>();

                    // Basic Settings
                    agent.speed = 5f * multiplier;
                    agent.acceleration = 250f * (multiplier / 2);
                    agent.angularSpeed = 1000f * (multiplier / 2);
                    agent.stoppingDistance = 0.2f; // Reduce stopping distance for precision
                    agent.autoBraking = true; // Set to false to manually control braking
                    agent.autoTraverseOffMeshLink = false; // Set to false to manually handle off mesh link traversal
                    agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance; // Use high quality obstacle avoidance for better precision

                    // Advanced Settings
                    agent.radius = 0.4f; // Reduce the radius for better precision around corners and doorways
                    agent.height = 2.0f; // Increase height for better clearance
                    agent.avoidancePriority = 50;

                    // Off Mesh Link Settings
                    agent.autoRepath = true; // Enable auto repathing for off mesh link traversal
                    agent.autoTraverseOffMeshLink = true; // Allow agent to traverse off mesh links automatically
                    agent.autoBraking = true; // Set autoBraking back to true for normal path following
                    
                    // Turn off auto braking on completion
                    agent.autoBraking = false;

                    // Always face the next waypoint directly
                    agent.updateRotation = true;
                    agent.updatePosition = true;

                    // Force agent to stay on navmesh
                    agent.updateUpAxis = false;

                    hasInitialized = true;
                }
                else
                {
                    agent = player.gameObject.GetComponent<NavMeshAgent>();
                    mls.LogMessage("didnt re-add nav mesh as it already existed");
                }

                runToLocations = FindObjectsOfType<EnemyVent>().Select(x => x.transform.position).ToList();
                runToLocations.Add(RoundManager.FindMainEntrancePosition());

                //start with it off
                ((Behaviour)(object)agent).enabled = false;
                moveTowardsDestination = false;
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }

        private static void startRunToNewPosition(PlayerControllerB player)
        {
            ((Behaviour)(object)agent).enabled = true;

            //sort runToLocations by distance to player
            var distances = runToLocations.Select(pos => Vector3.Distance(pos, player.transform.position)).ToArray();
            var positionDistancePairs = runToLocations.Zip(distances, (pos, dist) => new { Position = pos, Distance = dist });
            var sortedPositionDistancePairs = positionDistancePairs.OrderByDescending(pair => pair.Distance).ToArray();
            var sortedPositions = sortedPositionDistancePairs.Select(pair => pair.Position).ToList();

            int randomIndex = Random.Range(0, sortedPositions.Count/2);
            Vector3 randomLocation = sortedPositions[randomIndex];

            //set destination to the random location
            if (SetDestinationToPosition(randomLocation) == false)
            {
                if (sortedPositions.Count > 0)
                    sortedPositions.ElementAt(Random.Range(0, sortedPositions.Count / 2));
            }
        }

        private static bool SetDestinationToPosition(Vector3 position)
        {
            try
            {
                mls.LogMessage($"setting desitination to positon: {position}");
                position = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 1.75f);

                moveTowardsDestination = true;
                destination = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, -1f);
                mls.LogMessage($"destination is: {destination}");
                return true;
            }
            catch (Exception e)
            {
                mls.LogError(e);
                InitializeNaveMeshForPlayer();
            }

            return false;
        }

        #endregion
    }
}
