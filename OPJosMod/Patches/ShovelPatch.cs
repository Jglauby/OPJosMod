using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace OPJosMod.OPClientSide.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPrefix]
        private static bool hitShovelPatch(Shovel __instance)
        {
            return true;
        }
    }
}
