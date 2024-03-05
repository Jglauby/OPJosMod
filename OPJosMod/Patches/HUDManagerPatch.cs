using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("PingScan_performed")]
        [HarmonyPrefix]
        private static bool pingScan_performedPatch()
        {
            if (PlayerControllerBPatch.isSeeker)
            {
                mls.LogMessage("don't allow pinging as seeker");
                return false;
            }

            return true;
        }

        [HarmonyPatch("FillEndGameStats")]
        [HarmonyPrefix]
        private static void fillEndGameStatsPatchPre(HUDManager __instance, ref EndOfGameStats stats)
        {
            for (int i = 0; i < RoundManager.Instance.playersManager.allPlayerScripts.Length; i++)
            {
                StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Clear();

                if (RoundManager.Instance.playersManager.allPlayerScripts[i].isPlayerDead)
                {
                    mls.LogMessage($"player{i} is dead, so they lost");
                    StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Lost the game :(");
                }
                else
                {
                    mls.LogMessage($"player{i} won!");
                    StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add("Won the game!");
                }
            }
        }

        [HarmonyPatch("FillEndGameStats")]
        [HarmonyPostfix]
        private static void fillEndGameStatsPatchPost(HUDManager __instance, ref EndOfGameStats stats)
        {
            HUDManager.Instance.statsUIElements.allPlayersDeadOverlay.gameObject.SetActive(false);

            if (StartOfRound.Instance.localPlayerController.isPlayerDead)
            {
                HUDManager.Instance.statsUIElements.gradeLetter.text = "L";
            }
            else
            {
                HUDManager.Instance.statsUIElements.gradeLetter.text = "W";
            }
        }
    }
}
