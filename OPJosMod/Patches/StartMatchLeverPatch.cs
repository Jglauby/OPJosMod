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
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("EndGame")]
        [HarmonyPrefix]
        private static void patchEndGame(StartMatchLever __instance)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                GeneralUtil.ResetSpawnRatesForLevel();
            }
            else
            {
                mls.LogMessage("didn't reset spawn rates as you arent host");
            }
        }
    }
}
