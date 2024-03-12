using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Utils;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void startPatch(StartOfRound __instance)
        {
            __instance.profitQuotaMonitorText.text = "Hide N Seek!";
            __instance.deadlineMonitorText.text = "Have fun and such";           
            __instance.mapScreen.gameObject.SetActive(false);

            // in EndOfGame function call, could put this in a patch for that function?
            TimeOfDay.Instance.daysUntilDeadline = 4;
            TimeOfDay.Instance.timeUntilDeadline = 75500; //69 days lol
        }

        [HarmonyPatch("ReviveDeadPlayers")]
        [HarmonyPrefix]
        static void reviveDeadPlayersPatch(StartOfRound __instance)
        {
            mls.LogMessage("revive dead players patch hit in start of round class, reset isSeeker and isHider");

            PlayerControllerBPatch.resetRoleValues();
        }

        [HarmonyPatch("ShipLeave")]
        [HarmonyPrefix]
        public static void shipLeavePatch(StartOfRound __instance)
        {
            mls.LogMessage("ship leaving, reset isSeeker and isHider");
            mls.LogMessage($"current day time {TimeOfDay.Instance.currentDayTime} global time at end of day{TimeOfDay.Instance.globalTimeAtEndOfDay}");
            mls.LogMessage(PlayerControllerBPatch.isHider + "->ishider" + PlayerControllerBPatch.isSeeker + "->isSeekr");
            if (!StartOfRound.Instance.localPlayerController.isPlayerDead 
                && PlayerControllerBPatch.isHider
                && (TimeOfDay.Instance.currentDayTime + 10) > TimeOfDay.Instance.globalTimeAtEndOfDay)
            {
                mls.LogMessage("ship taking off but player isn't dead");
                StartOfRound.Instance.localPlayerController.transform.position = RoundManager.Instance.playersManager.playerSpawnPositions[0].position;
                HUDManager.Instance.DisplayTip("Round Over!", "Stay on ship");
            }
            PlayerControllerBPatch.resetRoleValues();
        }

        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        public static void StartOfRoundSuitPatch(StartOfRound __instance)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                int[] array = new int[4] { 1, 2, 3, 24 };
                foreach (int num in array)
                {
                    ReflectionUtils.InvokeMethod(StartOfRound.Instance, "SpawnUnlockable", new object[] { num });
                }
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "ResetShip")]
        [HarmonyPostfix]
        public static void ResetShipSuitPatch(StartOfRound __instance)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                StartOfRoundSuitPatch(__instance);
            }
        }

        [HarmonyPatch("SetPlanetsWeather")]
		[HarmonyPostfix]
        private static void setPlanetsWeatherPatch(ref SelectableLevel[] ___levels)
        {
            for (int i = 0; i < ___levels.Length; i++)
            {
                ___levels[i].currentWeather = (LevelWeatherType)(-1);
            }
        }
    }
}
