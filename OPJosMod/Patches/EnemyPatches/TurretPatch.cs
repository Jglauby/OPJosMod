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

        //[HarmonyPatch("CheckForPlayersInLineOfSight")]
        //[HarmonyPrefix]
        //private static bool checkForPlayersInLineOfSightPatch(Turret __instance, ref PlayerControllerB __result)
        //{
        //    if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
        //    {
        //        __result = __instance.CheckForPlayersInLineOfSight(3f);
        //
        //        //tryign to target you
        //        if (__result != null && __result.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
        //        {
        //            mls.LogMessage("dont let turret target you as ghost");
        //            return false;
        //        }
        //    }
        //
        //    return true;
        //}
    }
}
