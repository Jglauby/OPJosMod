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

        private static int lastFocusedPlayer = -1;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void updatePatch(NutcrackerEnemyAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == __instance.lastPlayerSeenMoving)
                {
                    mls.LogMessage("nutcracker should have seen client player");
                    __instance.lastPlayerSeenMoving = lastFocusedPlayer;
                }
                else
                {
                    lastFocusedPlayer = __instance.lastPlayerSeenMoving;
                }
            }
        }
    }
}
