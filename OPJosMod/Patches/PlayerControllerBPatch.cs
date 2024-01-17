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
            __instance.sprintTime = 19f; //default 5

            float walkingSprintRegen = 22f; 
            float standingSprintRegen = 13f; 

            FieldInfo isWalkingField = typeof(PlayerControllerB).GetField("isWalking", BindingFlags.NonPublic | BindingFlags.Instance);

            if (isWalkingField != null)
            {
                bool isWalkingValue = (bool)isWalkingField.GetValue(__instance);

                ///------modified snipit from update function----------
                if (__instance.isPlayerControlled && !__instance.isPlayerDead)
                {
                    float num2 = 1f;
                    if (!__instance.isSprinting && __instance.isMovementHindered <= 0)
                    {
                        if (!isWalkingValue)
                        {
                            __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter + Time.deltaTime / standingSprintRegen * num2, 0f, 1f);
                        }
                        else
                        {
                            __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter + Time.deltaTime / walkingSprintRegen * num2, 0f, 1f);
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
