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
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("FindExitPoint")]
        [HarmonyPrefix]
        private static bool findExitPointPatch()
        {
            if (PlayerControllerBPatch.isHider)
            {
                return false;
            }

            return true;
        }
    }
}
