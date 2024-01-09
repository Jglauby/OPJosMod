using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OPJosMod.BetterStamina.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {            
            FieldInfo isWalkingField = typeof(PlayerControllerB).GetField("isWalking", BindingFlags.NonPublic | BindingFlags.Instance);

            if (isWalkingField != null)
            {
                bool isWalkingValue = (bool)isWalkingField.GetValue(__instance);

                ///------modified snipit from update function----------
                if (__instance.isPlayerControlled && !__instance.isPlayerDead)
                {
                    __instance.localVisor.position = __instance.localVisorTargetPoint.position;
                    __instance.localVisor.rotation = Quaternion.Lerp(__instance.localVisor.rotation, __instance.localVisorTargetPoint.rotation, 53f * Mathf.Clamp(Time.deltaTime, 0.0167f, 20f));
                    float num2 = 1f;
                    if (__instance.drunkness > 0.02f)
                    {
                        num2 *= Mathf.Abs(StartOfRound.Instance.drunknessSpeedEffect.Evaluate(__instance.drunkness) - 1.25f);
                    }
                    if (__instance.isSprinting)
                    {
                        __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - Time.deltaTime / __instance.sprintTime * __instance.carryWeight * num2, 0f, 1f);
                        mls.LogMessage("BetterStamina(error): adjusting spritn meter");
                    }
                    else if (__instance.isMovementHindered > 0)
                    {
                        if (isWalkingValue)
                        {
                            __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - Time.deltaTime / __instance.sprintTime * num2 * 0.5f, 0f, 1f);
                        }
                    }
                    else
                    {
                        if (!isWalkingValue)
                        {
                            __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter + Time.deltaTime / (__instance.sprintTime + 4f) * num2, 0f, 1f);
                        }
                        else
                        {
                            __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter + Time.deltaTime / (__instance.sprintTime + 9f) * num2, 0f, 1f);
                        }
                        if (__instance.isExhausted && __instance.sprintMeter > 0.2f)
                        {
                            __instance.isExhausted = false;
                        }
                    }

                    __instance.sprintMeterUI.fillAmount = __instance.sprintMeter;
                }
            }
            else
            {
                mls.LogMessage("BetterStamina(error): failed to find private field");
            }
        }

        private static bool NearOtherPlayers(PlayerControllerB playerScript, float checkRadius = 10f)
        {
            playerScript.gameObject.layer = 0;
            bool result = Physics.CheckSphere(playerScript.transform.position, checkRadius, 8, QueryTriggerInteraction.Ignore);
            playerScript.gameObject.layer = 3;
            return result;
        }
    }
}
