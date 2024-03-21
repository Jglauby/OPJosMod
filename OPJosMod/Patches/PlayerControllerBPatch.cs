using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;

namespace OPJosMod.LagJutsu.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static bool godMode = true;

        private static float lastToggledTime = Time.time;
        private static float toggleDelay = 0.2f;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (((ButtonControl)Keyboard.current[ConfigVariables.DeathToggleButton]).wasPressedThisFrame)
            {
                if (Time.time - lastToggledTime > toggleDelay)
                {
                    string statusText = "";
                    switch (godMode)
                    {
                        case (true):
                            godMode = false;
                            statusText = "OFF";
                            break;
                        case (false):
                            godMode = true;
                            statusText = "ON";
                            break;
                    }
                    lastToggledTime = Time.time;
                    HUDManager.Instance.DisplayTip("God Mode", $"turned {statusText}");
                    mls.LogMessage($"god mode turned {statusText}");
                }
            }          
        }
    }
}
