using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Utils;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("PingScan_performed")]
        [HarmonyPrefix]
        private static bool pingScan_performedPatch()
        {
            if (PlayerControllerBPatch.isSeeker)
            {
                mls.LogMessage("don't allow pinging as seeker");
                return false;
            }

            return true;
        }
    }
}
