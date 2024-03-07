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
    [HarmonyPatch(typeof(CentipedeAI))]
    internal class CentipedeAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool updatePrePatch(CentipedeAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                //do nothing if ghost is closest to centipede and its hanging from ceiling
                if (EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId
                    && __instance.currentBehaviourStateIndex == 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
