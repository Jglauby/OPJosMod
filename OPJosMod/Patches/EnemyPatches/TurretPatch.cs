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
using UnityEngine.Rendering;

namespace OPJosMod.GhostMode.Enemy.Patches
{
    [HarmonyPatch(typeof(Turret))]
    internal class TurretPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch(typeof(Turret), "CheckForPlayersInLineOfSight")]
        [HarmonyPrefix]
        private static bool checkForPlayersInLineOfSightPatch(ref PlayerControllerB __result, Turret __instance, float radius = 2f, bool angleRangeCheck = false)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if (StartOfRound.Instance.localPlayerController.HasLineOfSightToPosition(__instance.transform.position))
                {
                    __result = null;

                    mls.LogMessage("Don't let turret target you as ghost");
                    return false;
                }
            }

            return true; // Allow the original method to execute in other cases
        }
    }
}
