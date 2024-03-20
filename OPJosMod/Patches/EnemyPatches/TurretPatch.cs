using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.SupahNinja.Utils;
using UnityEngine;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(Turret))]
    internal class TurretPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static RaycastHit hit;
        private static Ray shootRay;

        [HarmonyPatch(typeof(Turret), "CheckForPlayersInLineOfSight")]
        [HarmonyPrefix]
        private static bool checkForPlayersInLineOfSightPatch(Turret __instance, ref float radius, ref bool angleRangeCheck)
        {
            if (GeneralUtils.playerIsCrouching())
            {
                var enteringBerserkMode = ReflectionUtils.GetFieldValue<bool>(__instance, "enteringBerserkMode");

                Vector3 forward = __instance.aimPoint.forward;
                forward = Quaternion.Euler(0f, (float)(int)(0f - __instance.rotationRange) / radius, 0f) * forward;
                float num = __instance.rotationRange / radius * 2f;
                for (int i = 0; i <= 6; i++)
                {
                    shootRay = new Ray(__instance.centerPoint.position, forward);
                    if (Physics.Raycast(shootRay, out hit, 30f, 1051400, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform.CompareTag("Player"))
                        {
                            PlayerControllerB component = hit.transform.GetComponent<PlayerControllerB>();

                            if (component.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                                return false;

                            if (!(component == null))
                            {
                                if (angleRangeCheck && Vector3.Angle(component.transform.position + Vector3.up * 1.75f - __instance.centerPoint.position, __instance.forwardFacingPos.forward) > __instance.rotationRange)
                                {
                                    return true;
                                }

                                return component;
                            }

                            continue;
                        }

                        if ((__instance.turretMode == TurretMode.Firing || (__instance.turretMode == TurretMode.Berserk && !enteringBerserkMode)) && hit.transform.tag.StartsWith("PlayerRagdoll"))
                        {
                            Rigidbody component2 = hit.transform.GetComponent<Rigidbody>();
                            if (component2 != null)
                            {
                                component2.AddForce(forward.normalized * 42f, ForceMode.Impulse);
                            }
                        }
                    }

                    forward = Quaternion.Euler(0f, num / 6f, 0f) * forward;
                }
            }

            return true; // Allow the original method to execute in other cases
        }
    }
}
