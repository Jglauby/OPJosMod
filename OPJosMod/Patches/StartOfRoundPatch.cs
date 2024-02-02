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
        public static void updatePlayerVoiceEffectsPatch(StartOfRound __instance)
        {
            if (PlayerControllerBPatch.isGhostMode)
            {
                mls.LogMessage("UpdatePlayerVoiceEffects post hit");

                for (int i = 0; i < __instance.allPlayerScripts.Length; i++)
                {
                    PlayerControllerB playerControllerB2 = __instance.allPlayerScripts[i];

                    // Skip the local player
                    if (playerControllerB2 == GameNetworkManager.Instance.localPlayerController)
                    {
                        continue;
                    }

                    // Check if the player's voice chat objects are accessible
                    if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatIngameSettings._playerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                    {
                        // Attempt to refresh voice chat objects
                        __instance.RefreshPlayerVoicePlaybackObjects();
                        if (playerControllerB2.voicePlayerState == null || playerControllerB2.currentVoiceChatAudioSource == null)
                        {
                            mls.LogMessage($"Was not able to access voice chat object for player #{i}; {playerControllerB2.voicePlayerState == null}; {playerControllerB2.currentVoiceChatAudioSource == null}");
                            continue;
                        }
                    }

                    // Adjust audio properties for all players
                    mls.LogMessage($"adjusting volume for player{i}");
                    AudioSource currentVoiceChatAudioSource = StartOfRound.Instance.allPlayerScripts[i].currentVoiceChatAudioSource;
                    if (__instance.allPlayerScripts[i].isPlayerDead)
                    {
                        mls.LogMessage($"Player {i} is dead. Adjusting audio settings accordingly.");
                        //currentVoiceChatAudioSource.spatialBlend = 0f;
                        playerControllerB2.currentVoiceChatIngameSettings.set2D = true;
                        currentVoiceChatAudioSource.volume = 1f;
                    }
                    else
                    {
                        mls.LogMessage($"Player {i} is alive. Adjusting audio settings accordingly.");
                        //currentVoiceChatAudioSource.spatialBlend = 1f;
                        playerControllerB2.currentVoiceChatIngameSettings.set2D = false;
                        currentVoiceChatAudioSource.volume = 1f;
                    }

                    throw new Exception("dont call the regular UpdatePlayerVoiceEffects function");
                }
            }
        }
    }
}
