using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.ReviveCompany.CustomRpc;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OPJosMod.ReviveCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("EndOfGame")]
        [HarmonyPrefix]
        private static void patchEndOfGame()
        {
            if (!GlobalVariables.ModActivated)
                return;

            mls.LogMessage("reset bodies teleported list");
            GlobalVariables.DeadBodiesTeleported.Clear();
        }
    }
}
