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
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SandSpiderAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("TriggerChaseWithPlayer")]
        [HarmonyPrefix]
        static bool triggerChaseWithPlayerPatch(ref PlayerControllerB playerScript)
        {
            if (PlayerControllerBPatch.isGhostMode && StartOfRound.Instance.localPlayerController.playerClientId == playerScript.playerClientId && !ConfigVariables.enemiesDetectYou)
            {
                mls.LogMessage("spider supposed to trigger chase with player");
                return false;
            }

            return true;
        }
    }
}
