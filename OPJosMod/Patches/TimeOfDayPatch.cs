using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.HideNSeek.Config;
using System.Linq;
using UnityEngine;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void updatePatch(TimeOfDay __instance)
        {
            //set time speed, more players => longer days
            var playerCount = RoundManager.Instance.playersManager.allPlayerScripts.Where(x => !x.playerUsername.Contains("Player #")).ToArray().Length;
            var daySpeedIncrease = ConfigVariables.daySpeedMultiplier;
            __instance.globalTimeSpeedMultiplier = (daySpeedIncrease * 4) / playerCount;
        }
    }
}
