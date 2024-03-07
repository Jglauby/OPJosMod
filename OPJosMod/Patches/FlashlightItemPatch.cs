﻿using BepInEx.Logging;
using HarmonyLib;
using OPJosMod;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    internal class FlashlightItemPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void startPatch(FlashlightItem __instance)
        {
            mls.LogMessage($"flashlight version: {__instance.flashlightTypeID}");

            if(__instance.flashlightTypeID == (int)FlashlightTypes.NormalFlashlight)
            {
                __instance.itemProperties.requiresBattery = false;
                __instance.flashlightBulb.intensity = __instance.flashlightBulb.intensity / 5;
            }
        }
    }
}