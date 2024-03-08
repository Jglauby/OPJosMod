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
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        private static bool onTriggerEnterPatch(ref Collider other)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if (other.CompareTag("Player"))
                {
                    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                    if (!(component != GameNetworkManager.Instance.localPlayerController) && component != null && !component.isPlayerDead)
                    {
                        mls.LogMessage("ghost stepped on mine, do nothing");
                        return false;
                    }
                }
            }

            return true;
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        private static bool onTriggerExitPatch(ref Collider other)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if (other.CompareTag("Player"))
                {
                    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                    if (component != null && !component.isPlayerDead && !(component != GameNetworkManager.Instance.localPlayerController))
                    {
                        mls.LogMessage("ghost stepped off mine, do nothing");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
