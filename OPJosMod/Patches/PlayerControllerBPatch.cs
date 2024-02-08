using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.TheFlash.Utils;
using System;
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
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static float sprintMultiplier = 1.05f;
        private static float maxSprintSpeed = 20f;

        private static float walkMultiplier = 1.05f;
        private static float maxWalkSpeed = 7.5f;

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
                sprintMultiplier = 3f;
                maxSprintSpeed = 100f;
            }
        }
    }
}
