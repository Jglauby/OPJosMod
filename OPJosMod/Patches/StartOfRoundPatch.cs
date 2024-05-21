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

        [HarmonyPatch("openingDoorsSequence")]
        [HarmonyPrefix]
        private static void patchOpeningDoorsSequence()
        {
            if (!GlobalVariables.ModActivated)
                return;

            mls.LogMessage("round starting, reseting allowed revive count");
            GlobalVariables.DeadBodiesTeleported.Clear();

            if (ConfigVariables.RevivesPerLevel != null)
            {
                GlobalVariables.RemainingRevives = ConfigVariables.RevivesPerLevel.Value;
            }
            else
            {
                GlobalVariables.RemainingRevives = int.MaxValue;
            }
        }
    }
}
