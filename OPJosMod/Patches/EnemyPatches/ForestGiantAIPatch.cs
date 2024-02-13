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
    [HarmonyPatch(typeof(ForestGiantAI))]
    internal class ForestGiantAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        //[HarmonyPatch("DetectNoise")]
        //[HarmonyPrefix]
        //private static bool detectNoisePatch(ref Vector3 noisePosition)
        //{
        //    if (PlayerControllerBPatch.isGhostMode)
        //    {
        //        var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
        //        var playerIndex = StartOfRound.Instance.localPlayerController.playerClientId;
        //        var currentPlayer = allPlayerScripts[playerIndex];
        //
        //        if (GeneralUtils.twoPointsAreClose(noisePosition, currentPlayer.transform.position, 2f))
        //        {
        //            mls.LogMessage("noise was close to you, so don't detect this noise");
        //            return false;
        //        }
        //    }
        //
        //    return true;
        //}
    }
}
