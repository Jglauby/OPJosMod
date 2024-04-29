using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.MoreEnemies.Utils;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMod.MoreEnemies.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Disconnect")]
        [HarmonyPrefix]
        private static void patchDisconnect(GameNetworkManager __instance)
        {
            if (__instance.isHostingGame)
            {
                mls.LogMessage("DISCCONNECTING HIT");
                GeneralUtil.ResetSpawnRatesForLevel();
            }
            else
            {
                mls.LogMessage("didn't reset spawn rates as you arent host");
            }
        }
    }
}
