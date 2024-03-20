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
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(JesterAI))]
    internal class JesterAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void updatePatch(JesterAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if (__instance.currentBehaviourStateIndex == 2 || __instance.currentBehaviourStateIndex == 1)//if currently chasing a player/ or winding
                {
                    if (EnemyAIPatch.ghostOnlyPlayerInFacility())
                    { 
                        if (EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                        {
                            mls.LogMessage("swaping jester back to state 0");
                            __instance.SwitchToBehaviourState(1);
                        }
                    }
                }
            }
        }
    }
}
