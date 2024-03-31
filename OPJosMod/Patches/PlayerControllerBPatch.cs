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

            //vibrate player
            //__instance.StartCoroutine(vibratePlayer(__instance));
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
    }
}
