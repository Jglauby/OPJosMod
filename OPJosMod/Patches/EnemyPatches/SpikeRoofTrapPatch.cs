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
    [HarmonyPatch(typeof(SpikeRoofTrap))]
    internal class SpikeRoofTrapPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnTriggerStay")]
        [HarmonyPrefix]
        private static bool patchOnTriggerStay(SpikeRoofTrap __instance, ref Collider other)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                if (!__instance.trapActive || !__instance.slammingDown || Time.realtimeSinceStartup - __instance.timeSinceMovingUp < 0.75f)
                {
                    return true;
                }

                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                if (component != null && component == GameNetworkManager.Instance.localPlayerController && !component.isPlayerDead)
                {
                    //GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.down * 17f, spawnBody: true, CauseOfDeath.Crushing);
                    return false;
                }
            }

            return true;
        }
    }
}
