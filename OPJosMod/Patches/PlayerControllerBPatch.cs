﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace OPJosMod.ReviveCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            //mls.LogMessage($"Constants.ModActivated:{GlobalVariables.ModActivated}");
        }
    }
}
