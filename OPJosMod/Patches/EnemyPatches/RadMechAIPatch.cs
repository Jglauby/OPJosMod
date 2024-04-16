using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.GhostMode.Patches;
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
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

namespace OPJosMod.GhostMode.Enemy.Patches
{
    [HarmonyPatch(typeof(RadMechAI))]
    internal class RadMechAIPatch
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

        private static int visibleThreatsMask = 524296; //constant in the radMechAi class
        [HarmonyPatch("CheckSightForThreat")]
        [HarmonyPrefix]
        static bool patchCheckSightForThreat(RadMechAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                //esentially do the whole function before running
                Collider targetedThreatCollider = ReflectionUtils.GetFieldValue<Collider>(__instance, "targetedThreatCollider");
                int num = Physics.OverlapSphereNonAlloc(__instance.eye.position + __instance.eye.forward * 58f + -__instance.eye.up * 10f, 60f, RoundManager.Instance.tempColliderResults, visibleThreatsMask, QueryTriggerInteraction.Collide);
                Collider collider = null;
                RaycastHit hitInfo;
                IVisibleThreat component2;
                for (int i = 0; i < num; i++)
                {
                    if (RoundManager.Instance.tempColliderResults[i] == __instance.ownCollider)
                    {
                        continue;
                    }

                    if (RoundManager.Instance.tempColliderResults[i] == targetedThreatCollider && __instance.currentBehaviourStateIndex == 1)
                    {
                        collider = RoundManager.Instance.tempColliderResults[i];
                        continue;
                    }

                    float num2 = Vector3.Distance(__instance.eye.position, RoundManager.Instance.tempColliderResults[i].transform.position);
                    float num3 = Vector3.Angle(RoundManager.Instance.tempColliderResults[i].transform.position - __instance.eye.position, __instance.eye.forward);
                    if (num2 > 2f && num3 > __instance.fov)
                    {
                        continue;
                    }

                    if (Physics.Linecast(__instance.transform.position + Vector3.up * 0.7f, RoundManager.Instance.tempColliderResults[i].transform.position + Vector3.up * 0.5f, out hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                    {
                        if (__instance.debugEnemyAI)
                        {
                            Debug.DrawRay(hitInfo.point, Vector3.up * 0.5f, Color.magenta, __instance.AIIntervalTime);
                        }

                        continue;
                    }

                    EnemyAI component = RoundManager.Instance.tempColliderResults[i].transform.GetComponent<EnemyAI>();
                    if ((!(component != null) || !(component.GetType() == typeof(RadMechAI))) && RoundManager.Instance.tempColliderResults[i].transform.TryGetComponent<IVisibleThreat>(out component2))
                    {
                        float visibility = component2.GetVisibility();
                        if (!(visibility < 0.2f) && (!(visibility <= 0.58f) || !(num2 > 30f)))
                        {
                            if (component2 is PlayerControllerB)
                            {
                                PlayerControllerB playerDetected = component2 as PlayerControllerB;
                                //mls.LogMessage($"player detected clientid: {playerDetected.playerClientId}");
                                if (playerDetected != null && playerDetected.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
