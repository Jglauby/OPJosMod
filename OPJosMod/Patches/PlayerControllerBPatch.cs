using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace OPJosMod.GodMode.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void patchKillPlayer(PlayerControllerB __instance, ref int deathAnimation, ref bool spawnBody)
        {
            mls.LogMessage("should've died but didn't");
            FieldInfo wasUnderWaterLastFrameField = typeof(PlayerControllerB).GetField("wasUnderwaterLastFrame", BindingFlags.NonPublic | BindingFlags.Instance);

            if (wasUnderWaterLastFrameField != null)
            {
                bool wasUnderWaterLastFrameValue = (bool)wasUnderWaterLastFrameField.GetValue(__instance);

                //killplayer function
                if (__instance.IsOwner && !__instance.isPlayerDead && __instance.AllowPlayerDeath())
                {
                    __instance.isPlayerDead = true;
                    __instance.isPlayerControlled = false;
                    __instance.thisPlayerModelArms.enabled = false;
                    __instance.localVisor.position = __instance.playersManager.notSpawnedPosition.position;
                    __instance.DisablePlayerModel(__instance.gameObject);
                    __instance.isInsideFactory = false;
                    __instance.IsInspectingItem = false;
                    __instance.inTerminalMenu = false;
                    __instance.twoHanded = false;
                    __instance.carryWeight = 1f;
                    __instance.fallValue = 0f;
                    __instance.fallValueUncapped = 0f;
                    __instance.takingFallDamage = false;
                    __instance.isSinking = false;
                    __instance.isUnderwater = false;
                    StartOfRound.Instance.drowningTimer = 1f;
                    HUDManager.Instance.setUnderwaterFilter = false;
                    wasUnderWaterLastFrameField.SetValue(__instance, false);
                    __instance.sourcesCausingSinking = 0;
                    __instance.sinkingValue = 0f;
                    __instance.hinderedMultiplier = 1f;
                    __instance.isMovementHindered = 0;
                    __instance.inAnimationWithEnemy = null;
                    UnityEngine.Object.FindObjectOfType<Terminal>().terminalInUse = false;
                    ChangeAudioListenerToObject(__instance, __instance.playersManager.spectateCamera.gameObject);
                    SoundManager.Instance.SetDiageticMixerSnapshot();
                    HUDManager.Instance.SetNearDepthOfFieldEnabled(enabled: true);
                    HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", value: false);
                    HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
                    HUDManager.Instance.HideHUD(hide: true);
                    StopHoldInteractionOnTrigger(__instance);
                    //KillPlayerServerRpc((int)playerClientId, spawnBody, bodyVelocity, (int)causeOfDeath, deathAnimation);

                    if (spawnBody)
                    {
                        __instance.SpawnDeadBody((int)__instance.playerClientId, __instance.velocityLastFrame, (int)__instance.causeOfDeath, __instance, deathAnimation);
                    }

                    StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);
                    __instance.isInGameOverAnimation = 1.5f;
                    __instance.cursorTip.text = "";
                    __instance.cursorIcon.enabled = false;
                    __instance.DropAllHeldItems(true);
                    __instance.DisableJetpackControlsLocally();
                }
            }

            throw new Exception("actually don't kill");
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPostfix]
        static void patchDamagePlayer(PlayerControllerB __instance)
        {
            __instance.health = 100;
            HUDManager.Instance.UpdateHealthUI(__instance.health, false);         
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

        //one for one private functions that existin player controller b
        private static void ChangeAudioListenerToObject(PlayerControllerB __instance, GameObject addToObject)
        {
            __instance.activeAudioListener.transform.SetParent(addToObject.transform);
            __instance.activeAudioListener.transform.localEulerAngles = Vector3.zero;
            __instance.activeAudioListener.transform.localPosition = Vector3.zero;
            StartOfRound.Instance.audioListener = __instance.activeAudioListener;
        }

        //one for one private functions that exist in player controller b
        private static void StopHoldInteractionOnTrigger(PlayerControllerB __instance)
        {
            HUDManager.Instance.holdFillAmount = 0f;
            if (__instance.previousHoveringOverTrigger != null)
            {
                __instance.previousHoveringOverTrigger.StopInteraction();
            }
            if (__instance.hoveringOverTrigger != null)
            {
                __instance.hoveringOverTrigger.StopInteraction();
            }
        }

        //slightly modified version of ReviveDeadPlayers from the StartOfRound object
        private static void ReviveDeadPlayer(PlayerControllerB __instance)
        {
            Debug.Log("Reviving players A");
            __instance.ResetPlayerBloodObjects(__instance.isPlayerDead);
            if (!__instance.isPlayerDead && !__instance.isPlayerControlled)
            {
                return;
            }
            __instance.isClimbingLadder = false;
            __instance.ResetZAndXRotation();
            __instance.thisController.enabled = true;
            __instance.health = 100;
            __instance.disableLookInput = false;
            Debug.Log("Reviving players B");
            if (__instance.isPlayerDead)
            {
                __instance.isPlayerDead = false;
                __instance.isPlayerControlled = true;
                __instance.isInElevator = true;
                __instance.isInHangarShipRoom = true;
                __instance.isInsideFactory = false;
                __instance.wasInElevatorLastFrame = false;
                __instance.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;

                if(__instance.deadBody != null)
                {
                    __instance.TeleportPlayer(__instance.deadBody.transform.position);
                }
                else
                {
                    __instance.TeleportPlayer(__instance.transform.position);
                }

                __instance.setPositionOfDeadPlayer = false;
                __instance.DisablePlayerModel(__instance.gameObject, enable: true, disableLocalArms: true);
                __instance.helmetLight.enabled = false;
                Debug.Log("Reviving players C");
                __instance.Crouch(crouch: false);
                __instance.criticallyInjured = false;
                if (__instance.playerBodyAnimator != null)
                {
                    __instance.playerBodyAnimator.SetBool("Limp", value: false);
                }
                __instance.bleedingHeavily = false;
                __instance.activatingItem = false;
                __instance.twoHanded = false;
                __instance.inSpecialInteractAnimation = false;
                __instance.disableSyncInAnimation = false;
                __instance.inAnimationWithEnemy = null;
                __instance.holdingWalkieTalkie = false;
                __instance.speakingToWalkieTalkie = false;
                Debug.Log("Reviving players D");
                __instance.isSinking = false;
                __instance.isUnderwater = false;
                __instance.sinkingValue = 0f;
                __instance.statusEffectAudio.Stop();
                __instance.DisableJetpackControlsLocally();
                __instance.health = 100;
                Debug.Log("Reviving players E");
                __instance.mapRadarDotAnimator.SetBool("dead", value: false);
                if (__instance.IsOwner)
                {
                    HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
                    __instance.hasBegunSpectating = false;
                    HUDManager.Instance.RemoveSpectateUI();
                    HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                    __instance.hinderedMultiplier = 1f;
                    __instance.isMovementHindered = 0;
                    __instance.sourcesCausingSinking = 0;
                    Debug.Log("Reviving players E2");
                    //__instance.reverbPreset = new ReverbPreset();
                }
            }

            Debug.Log("Reviving players F");
            SoundManager.Instance.earsRingingTimer = 0f;
            __instance.voiceMuffledByEnemy = false;
            //SoundManager.Instance.playerVoicePitchTargets[__instance.playerLevelNumber] = 1f;
            //SoundManager.Instance.SetPlayerPitch(1f, __instance.playerLevelNumber);
            if (__instance.currentVoiceChatIngameSettings == null)
            {
                //RefreshPlayerVoicePlaybackObjects();
            }
            if (__instance.currentVoiceChatIngameSettings != null)
            {
                if (__instance.currentVoiceChatIngameSettings.voiceAudio == null)
                {
                    __instance.currentVoiceChatIngameSettings.InitializeComponents();
                }
                if (__instance.currentVoiceChatIngameSettings.voiceAudio == null)
                {
                    return;
                }
                __instance.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
            }

            PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
            playerControllerB.bleedingHeavily = false;
            playerControllerB.criticallyInjured = false;
            playerControllerB.playerBodyAnimator.SetBool("Limp", value: false);
            playerControllerB.health = 100;
            HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
            playerControllerB.spectatedPlayerScript = null;
            HUDManager.Instance.audioListenerLowPass.enabled = false;
            HUDManager.Instance.HideHUD(hide: false);

            DeadBodyInfo[] array2 = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>();
            for (int k = 0; k < array2.Length; k++)
            {
                UnityEngine.Object.Destroy(array2[k].gameObject);
            }

            //StartOfRound startOfRound = new StartOfRound();
            //startOfRound.UpdatePlayerVoiceEffects();

            //ResetMiscValues();
        }
    }
}
