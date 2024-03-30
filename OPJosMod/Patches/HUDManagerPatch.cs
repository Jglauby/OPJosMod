using BepInEx;
using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.InputSystem.DefaultInputActions;

namespace OPJosMod.GhostMode.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static int livingPlayersCount = 0;

        public static List<string> playerNotes = new List<string>();
        public static TextMeshProUGUI mcPlayerNotes;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void updatePatch(HUDManager __instance)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null ||
               GameNetworkManager.Instance.localPlayerController == null)
                {
                    return;
                }

                if (livingPlayersCount != RoundManager.Instance.playersManager.livingPlayers)
                {
                    mls.LogMessage("Adding boxes");

                    __instance.gameOverAnimator.SetTrigger("gameOver");
                    __instance.spectatingPlayerText.text = "";
                    __instance.holdButtonToEndGameEarlyText.text = "";
                    __instance.holdButtonToEndGameEarlyMeter.gameObject.SetActive(false);
                    __instance.holdButtonToEndGameEarlyVotesText.text = "";

                    updateBoxesSpectateUI(__instance);
                }

                FieldInfo updateSpectateBoxesIntervalField = typeof(HUDManager).GetField("updateSpectateBoxesInterval", BindingFlags.NonPublic | BindingFlags.Instance);
                float updateSpectateBoxesIntervalValue = (float)updateSpectateBoxesIntervalField.GetValue(__instance);
                MethodInfo updateSpectateBoxSpeakerIconsMethod = typeof(HUDManager).GetMethod("UpdateSpectateBoxSpeakerIcons", BindingFlags.NonPublic | BindingFlags.Instance);
                if (updateSpectateBoxesIntervalValue >= 0.35f)
                {
                    updateSpectateBoxesIntervalField.SetValue(__instance, 0f);
                    updateSpectateBoxSpeakerIconsMethod.Invoke(__instance, null);
                }
                else
                {
                    updateSpectateBoxesIntervalValue += Time.deltaTime;
                    updateSpectateBoxesIntervalField.SetValue(__instance, updateSpectateBoxesIntervalValue);
                }

                livingPlayersCount = RoundManager.Instance.playersManager.livingPlayers;
            }          
        }

        public static void updateBoxesSpectateUI(HUDManager __instance)
        {
            PlayerControllerB playerScript;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                playerScript = StartOfRound.Instance.allPlayerScripts[i];
                if (!playerScript.isPlayerDead)
                {
                    continue;
                }

                FieldInfo spectatingPlayerBoxesField = typeof(HUDManager).GetField("spectatingPlayerBoxes", BindingFlags.NonPublic | BindingFlags.Instance);
                var spectatingPlayerBoxes = (Dictionary<Animator, PlayerControllerB>)spectatingPlayerBoxesField.GetValue(__instance);

                FieldInfo fieldInfoYOffsetAmount = typeof(HUDManager).GetField("yOffsetAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                float yOffsetAmount = (float)fieldInfoYOffsetAmount.GetValue(__instance);

                FieldInfo fieldInfoBoxesAdded = typeof(HUDManager).GetField("boxesAdded", BindingFlags.NonPublic | BindingFlags.Instance);
                int boxesAdded = (int)fieldInfoBoxesAdded.GetValue(__instance);

                if (spectatingPlayerBoxes.Values.Contains(playerScript))
                {
                    // If the player is already in the spectating player boxes, adjust its position
                    GameObject gameObject = spectatingPlayerBoxes.FirstOrDefault((KeyValuePair<Animator, PlayerControllerB> x) => x.Value == playerScript).Key.gameObject;
                    if (!gameObject.activeSelf)
                    {
                        RectTransform component2 = gameObject.GetComponent<RectTransform>();
                        component2.anchoredPosition = new Vector2(component2.anchoredPosition.x, yOffsetAmount);
                        fieldInfoBoxesAdded.SetValue(__instance, boxesAdded++);
                        gameObject.SetActive(value: true);
                        fieldInfoYOffsetAmount.SetValue(__instance, yOffsetAmount - 70f);
                    }
                }
                else
                {
                    // If the player is not in the spectating player boxes, add it
                    GameObject gameObject = UnityEngine.Object.Instantiate(__instance.spectatingPlayerBoxPrefab, __instance.SpectateBoxesContainer, worldPositionStays: false);
                    gameObject.SetActive(value: true);
                    RectTransform component3 = gameObject.GetComponent<RectTransform>();
                    component3.anchoredPosition = new Vector2(component3.anchoredPosition.x, yOffsetAmount);
                    fieldInfoYOffsetAmount.SetValue(__instance, yOffsetAmount - 70f);
                    fieldInfoBoxesAdded.SetValue(__instance, boxesAdded++);

                    //__instance.spectatingPlayerBoxes.Add(gameObject.GetComponent<Animator>(), playerScript);
                    spectatingPlayerBoxes.Add(gameObject.GetComponent<Animator>(), playerScript);
                    spectatingPlayerBoxesField.SetValue(__instance, spectatingPlayerBoxes);

                    gameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerScript.playerUsername;
                    if (!GameNetworkManager.Instance.disableSteam)
                    {
                        HUDManager.FillImageWithSteamProfile(gameObject.GetComponent<RawImage>(), playerScript.playerSteamId);
                    }
                }

                mls.LogMessage($"boxes count:{spectatingPlayerBoxes.Count}");
            }
        }

        [HarmonyPatch("FillEndGameStats")]
        [HarmonyPrefix]
        private static void fillEndGameStatsPatchPre(HUDManager __instance, ref EndOfGameStats stats)
        {
            ////populate stats normally
            //for (int i = 0; i < RoundManager.Instance.playersManager.allPlayerScripts.Length; i++)
            //{
            //    if (RoundManager.Instance.playersManager.allPlayerScripts[i].playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            //    {
            //        StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Clear();
            //
            //        foreach (string note in playerNotes)
            //        {
            //            StartOfRound.Instance.gameStats.allPlayerStats[i].playerNotes.Add(note);
            //        }
            //    }
            //}
            //
            ////populate stats if you have more company
            //for (int i = 0; i < __instance.statsUIElements.playerNotesText.Length; i++)
            //{
            //    var player = RoundManager.Instance.playersManager.allPlayerScripts[i];
            //    if (player.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            //    {
            //        var text = __instance.statsUIElements.playerNotesText[i];
            //
            //        if (!player.playerUsername.Contains("Player #"))
            //        {
            //            text.text = mcPlayerNotes.text;
            //        }
            //        else
            //        {
            //            mls.LogMessage($"dont put stats for player {i}");
            //        }
            //    }
            //}
        }
    }
}
