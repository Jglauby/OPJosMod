using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.MoreEnemies;
using OPJosMod.MoreEnemies.CustomRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.MoreEnemies.Patches
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
