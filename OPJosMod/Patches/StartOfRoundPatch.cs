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
            GeneralUtil.ResetAllPlayerInfos();
        }

        [HarmonyPatch("openingDoorsSequence")]
        [HarmonyPrefix]
        private static void patchOpeningDoorsSequence()
        {
            if (!GlobalVariables.ModActivated)
                return;

            mls.LogMessage("round starting, reseting allowed revive count");
            GeneralUtil.ResetAllPlayerInfos();

            setStartingRevives();
        }

        private static void setStartingRevives()
        {
            var totalPlayerCount = RoundManager.Instance.playersManager.allPlayerScripts.Where(x => x.isPlayerControlled).Count();
            GlobalVariables.AmountOfTimesRevived = 0;
            if (ConfigVariables.LimitedRevives)
            {
                if (ConfigVariables.HardAmountOfLives != 0)
                    GlobalVariables.RemainingRevives = ConfigVariables.HardAmountOfLives;
                else 
                    GlobalVariables.RemainingRevives = Mathf.RoundToInt(totalPlayerCount * ConfigVariables.RevivesPerLevelMultiplier);
            }
            else
            {
                GlobalVariables.RemainingRevives = int.MaxValue;
            }
        }
    }
}
