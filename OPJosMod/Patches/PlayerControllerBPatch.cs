using BepInEx.Logging;
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
using UnityEngine.Rendering;

namespace OPJosMod.GhostMode.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static bool allowKill = true;

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void patchKillPlayer(PlayerControllerB __instance)
        {
            if (allowKill)
            {
                allowKill = false;
                mls.LogMessage("called kill player");
            }
            else
            {
                mls.LogMessage("didn't allow kill, aka player should be dead on server already");
                throw new Exception("dont kill player again");
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updatePatch(PlayerControllerB __instance)
        {
            if (!allowKill)
            {
                __instance.sprintMeter = 1f;
            }
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPostfix]
        static void damagePlayerPatch(PlayerControllerB __instance)
        {
            if (!allowKill)
            {
                __instance.health = 100;
                HUDManager.Instance.UpdateHealthUI(__instance.health, false);
            }
        }

        [HarmonyPatch("ActivateItem_performed")]
        [HarmonyPostfix]
        static void patchActivateItem_performed(PlayerControllerB __instance)
        {            
            if (__instance.IsOwner && __instance.isPlayerDead && (!__instance.IsServer || __instance.isHostPlayerObject))
            {
                mls.LogMessage("attempting to revive");
                ReviveDeadPlayer(__instance);
            }
        }

        private static void ReviveDeadPlayer(PlayerControllerB __instance)
        {
            mls.LogMessage("add player back server");

            try
            {
                Vector3 respawnLocation = new Vector3(0, 0, 0);
                if (__instance.deadBody != null)
                {
                    respawnLocation = __instance.deadBody.transform.position;
                }
                else
                {
                    respawnLocation = __instance.transform.position;
                }

                var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                var playerIndex = (int)__instance.playerClientId;

                mls.LogMessage($"Reviving player {playerIndex}");

                allPlayerScripts[playerIndex].ResetPlayerBloodObjects(allPlayerScripts[playerIndex].isPlayerDead);

                allPlayerScripts[playerIndex].isClimbingLadder = false;
                allPlayerScripts[playerIndex].ResetZAndXRotation();
                allPlayerScripts[playerIndex].thisController.enabled = true;
                allPlayerScripts[playerIndex].health = 100;
                allPlayerScripts[playerIndex].disableLookInput = false;

                if (allPlayerScripts[playerIndex].isPlayerDead)
                {
                    // Revival process for the specified player
                    allPlayerScripts[playerIndex].isPlayerDead = false;
                    allPlayerScripts[playerIndex].isPlayerControlled = true;
                    allPlayerScripts[playerIndex].isInElevator = true;
                    allPlayerScripts[playerIndex].isInHangarShipRoom = true;
                    allPlayerScripts[playerIndex].isInsideFactory = false;
                    allPlayerScripts[playerIndex].wasInElevatorLastFrame = false;
                    StartOfRound.Instance.SetPlayerObjectExtrapolate(enable: false);
                    allPlayerScripts[playerIndex].TeleportPlayer(respawnLocation);
                    allPlayerScripts[playerIndex].setPositionOfDeadPlayer = false;
                    allPlayerScripts[playerIndex].DisablePlayerModel(StartOfRound.Instance.allPlayerObjects[playerIndex], enable: true, disableLocalArms: true);
                    allPlayerScripts[playerIndex].helmetLight.enabled = false;

                    allPlayerScripts[playerIndex].Crouch(crouch: false);
                    allPlayerScripts[playerIndex].criticallyInjured = false;

                    if (allPlayerScripts[playerIndex].playerBodyAnimator != null)
                    {
                        allPlayerScripts[playerIndex].playerBodyAnimator.SetBool("Limp", value: false);
                    }

                    allPlayerScripts[playerIndex].bleedingHeavily = false;
                    allPlayerScripts[playerIndex].activatingItem = false;
                    allPlayerScripts[playerIndex].twoHanded = false;
                    allPlayerScripts[playerIndex].inSpecialInteractAnimation = false;
                    allPlayerScripts[playerIndex].disableSyncInAnimation = false;
                    allPlayerScripts[playerIndex].inAnimationWithEnemy = null;
                    allPlayerScripts[playerIndex].holdingWalkieTalkie = false;
                    allPlayerScripts[playerIndex].speakingToWalkieTalkie = false;

                    allPlayerScripts[playerIndex].isSinking = false;
                    allPlayerScripts[playerIndex].isUnderwater = false;
                    allPlayerScripts[playerIndex].sinkingValue = 0f;
                    allPlayerScripts[playerIndex].statusEffectAudio.Stop();
                    allPlayerScripts[playerIndex].DisableJetpackControlsLocally();
                    allPlayerScripts[playerIndex].health = 100;

                    //allPlayerScripts[playerIndex].mapRadarDotAnimator.SetBool("dead", value: false);

                    if (allPlayerScripts[playerIndex].IsOwner)
                    {
                        HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
                        allPlayerScripts[playerIndex].hasBegunSpectating = false;
                        HUDManager.Instance.RemoveSpectateUI();
                        HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                        allPlayerScripts[playerIndex].hinderedMultiplier = 1f;
                        allPlayerScripts[playerIndex].isMovementHindered = 0;
                        allPlayerScripts[playerIndex].sourcesCausingSinking = 0;

                        allPlayerScripts[playerIndex].reverbPreset = StartOfRound.Instance.shipReverb;
                    }
                }

                // Additional revival steps for the specified player
                SoundManager.Instance.earsRingingTimer = 0f;
                allPlayerScripts[playerIndex].voiceMuffledByEnemy = false;
                SoundManager.Instance.playerVoicePitchTargets[playerIndex] = 1f;
                SoundManager.Instance.SetPlayerPitch(1f, playerIndex);

                if (allPlayerScripts[playerIndex].currentVoiceChatIngameSettings == null)
                {
                    StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
                }

                if (allPlayerScripts[playerIndex].currentVoiceChatIngameSettings != null &&
                    allPlayerScripts[playerIndex].currentVoiceChatIngameSettings.voiceAudio != null)
                {
                    allPlayerScripts[playerIndex].currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
                }

                PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
                playerControllerB.bleedingHeavily = false;
                playerControllerB.criticallyInjured = false;
                playerControllerB.playerBodyAnimator.SetBool("Limp", value: false);
                playerControllerB.health = 100;
                HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
                playerControllerB.spectatedPlayerScript = null;
                HUDManager.Instance.audioListenerLowPass.enabled = false;

                StartOfRound.Instance.SetSpectateCameraToGameOverMode(enableGameOver: false, playerControllerB);               

                StartOfRound.Instance.UpdatePlayerVoiceEffects();

                //update the game to have no darkness
                __instance.nightVision.type = (LightType)2;
                __instance.nightVision.intensity = 44444f;
                __instance.nightVision.range = 99999f;
                __instance.nightVision.shadowStrength = 0f;
                __instance.nightVision.bounceIntensity = 5555f;
                __instance.nightVision.innerSpotAngle = 999f;
                __instance.nightVision.spotAngle = 9999f;
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }

            mls.LogMessage("Player respawned on server: " + (int)__instance.playerClientId);            
        }
    }
}
