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

namespace OPJosMod.TheFlash.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch : MonoBehaviour
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;

            sprintMultiplier = defaultSprintMultiplier;
            maxSprintSpeed = defaultMaxSprintSpeed;
        }

        private static float defaultSprintMultiplier = 1.05f;
        private static float defaultMaxSprintSpeed = 20f;
        private static float increasedSprintMultiplier = 4f;
        private static float increasedMaxSprintSpeed = 60f;
        private static float sprintMultiplier;
        private static float maxSprintSpeed;

        private static float walkMultiplier = 1.05f;
        private static float maxWalkSpeed = 7.5f;

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
                    var newForce = (float)currentValue * walkMultiplier;

                    if(newForce < maxWalkSpeed)
                        sprintMultiplierField.SetValue(__instance, newForce);
                }
            }

            if (((ButtonControl)Keyboard.current[(Key)0x20]).wasPressedThisFrame && !__instance.inTerminalMenu)//R was pressed
            {
                if(adjustingSpeed == false)
                {
                    adjustingSpeed = true;
                    __instance.StartCoroutine(toggleSpeed(__instance));
                    adjustingSpeed = false;
                }
            }
        }

        private static IEnumerator toggleSpeed(PlayerControllerB __instance)
        {
            yield return new WaitForSeconds(1f);

            if (speedMode == 0)
            {
                speedMode = 1;
                sprintMultiplier = increasedSprintMultiplier;
                maxSprintSpeed = increasedMaxSprintSpeed;
            }
            else if (speedMode == 1)
            {
                speedMode = 0;
                sprintMultiplier = defaultSprintMultiplier;
                maxSprintSpeed = defaultMaxSprintSpeed;
            }
        }
    }
}
