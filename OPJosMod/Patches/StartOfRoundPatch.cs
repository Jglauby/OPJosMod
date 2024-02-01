using BepInEx.Logging;
using Dissonance.Integrations.Unity_NFGO;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
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
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

namespace OPJosMod.GhostMode.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("ReviveDeadPlayers")]
        [HarmonyPrefix]
        static void reviveDeadPlayersPatch(StartOfRound __instance)
        {
            mls.LogMessage("revive dead players patch hit in start of round class");

            PlayerControllerBPatch.resetGhostModeVars(__instance.localPlayerController);
        }

        [HarmonyPatch("OnPlayerConnectedClientRpc")]
        [HarmonyPostfix]
        static void onPlayerConnectedClientRpcPatch(StartOfRound __instance)
        {
            mls.LogMessage("player connected patch hit in start of round class");

            PlayerControllerBPatch.resetGhostModeVars(__instance.localPlayerController);
        }

        [HarmonyPatch("UpdatePlayerVoiceEffects")]
        [HarmonyPrefix]
        static void updatePlayerVoiceEffectsPatch(PlayerControllerB __instance)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                mls.LogMessage("UpdatePlayerVoiceEffects post hit");

                for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    PlayerControllerB playerControllerB2 = StartOfRound.Instance.allPlayerScripts[i];

                    // Check if the player is the local player or controlled by the local player
                    if (playerControllerB2 == GameNetworkManager.Instance.localPlayerController)
                    {
                        // Skip the local player
                        continue;
                    }

                    // Check if the player's voice chat objects are accessible
                    if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatIngameSettings._playerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                    {
                        // Attempt to refresh voice chat objects
                        StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
                        if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                        {
                            mls.LogMessage($"Was not able to access voice chat object for player #{i}; {playerControllerB2.voicePlayerState == null}; {playerControllerB2.currentVoiceChatAudioSource == null}");
                            continue;
                        }
                    }

                    // Adjust audio properties for all players
                    mls.LogMessage($"adjusting volume for player{i}");
                    AudioSource currentVoiceChatAudioSource = StartOfRound.Instance.allPlayerScripts[i].currentVoiceChatAudioSource;
                    if (StartOfRound.Instance.allPlayerScripts[i].isPlayerDead)
                    {
                        currentVoiceChatAudioSource.spatialBlend = 0f;
                        playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                        currentVoiceChatAudioSource.volume = 1f;
                    }
                    else
                    {
                        currentVoiceChatAudioSource.spatialBlend = 1f;
                        playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                        currentVoiceChatAudioSource.volume = 1f;
                    }

                    throw new Exception("dont call the regular UpdatePlayerVoiceEffects function");
                }
            }
        }
    }
}
