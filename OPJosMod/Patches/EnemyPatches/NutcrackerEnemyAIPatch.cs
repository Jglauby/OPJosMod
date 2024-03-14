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
    [HarmonyPatch(typeof(NutcrackerEnemyAI))]
    internal class NutcrackerEnemyAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("SwitchTargetToPlayer")]
        [HarmonyPrefix]
        private static bool switchTargetToPlayerPatch(NutcrackerEnemyAI __instance, ref int playerId)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if ((int)StartOfRound.Instance.localPlayerController.playerClientId == playerId)
                {
                    mls.LogMessage("kept nutcracker from switching target to current player");
                    __instance.StopInspection();
                    __instance.currentBehaviourStateIndex = 0;
                    return false;
                }
            }

            return true;
        }    
    }
}
