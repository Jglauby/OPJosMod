using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
                //mls.LogMessage("don't allow pinging as seeker");
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

        [HarmonyPatch("ApplyPenalty")]
        [HarmonyPrefix]
        private static bool applyPenaltyPatch(HUDManager __instance, ref int playersDead, ref int bodiesInsured)
        {
            return false;
        }

        public static void CustomDisplayTip(string headerText, string bodyText, bool makeSound = true)
        {
            HUDManager instance = HUDManager.Instance;
            ((TMP_Text)instance.tipsPanelHeader).text = headerText;
            ((TMP_Text)instance.tipsPanelBody).text = bodyText;

            instance.tipsPanelAnimator.SetTrigger("TriggerHint");

            if (makeSound)
                RoundManager.PlayRandomClip(instance.UIAudio, instance.tipsSFX, randomize: false);
        }

        public static void CustomDisplayBigMessage(string maingText, bool makeSound = true)
        {
            HUDManager.Instance.profitQuotaDaysLeftText.color = new Color(0.8431f, 0.2314f, 0.2196f, 1.0f);
            HUDManager.Instance.profitQuotaDaysLeftText.fontSize = 46;
            HUDManager.Instance.profitQuotaDaysLeftText.fontStyle = FontStyles.Bold;

            //original scale = (1.00f, 1.04f, 1.00f)
            HUDManager.Instance.profitQuotaDaysLeftText.rectTransform.localScale = new Vector3(1.00f, 1.04f, 1.02f);
            HUDManager.Instance.profitQuotaDaysLeftText.text = "TO MEET PROFIT QUOTA";

            //og spot(-21.80, -59.19, 12.15)
            Vector3 offset = new Vector3(-21.78f, -59.48f, 12.15f);
            HUDManager.Instance.profitQuotaDaysLeftText.transform.position = offset;

            HUDManager.Instance.profitQuotaDaysLeftText2.text = maingText;
            HUDManager.Instance.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeft");

            if (makeSound)
                HUDManager.Instance.UIAudio.PlayOneShot(HUDManager.Instance.OneDayToMeetQuotaSFX);
        }
    }
}
