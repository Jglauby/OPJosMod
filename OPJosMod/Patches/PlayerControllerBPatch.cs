using BepInEx.Logging;
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
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static bool allowKill = true;
        private static bool isGhostMode = false;
        private static Coroutine jumpCoroutine;

        private static Vector3 deathLocation;
        private static int consecutiveDeathExceptions = 0;
        private static int maxConsecutiveDeathExceptions = 3;
        private static float exceptionCooldownTime = 2f; 
        private static float lastExceptionTime = 0f;

        private static Vector3 getTeleportLocation(PlayerControllerB __instance)
        {
            var result = new Vector3(0, 0, 0);
            if (__instance.deadBody != null)
            {
                result = __instance.deadBody.transform.position;
            }
            else
            {
                result = deathLocation;
            }

            return result;
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void startPatch(ref Light ___nightVision)
        {
            ___nightVision.type = (LightType)2;
            ___nightVision.intensity = 44444f;
            ___nightVision.range = 99999f;
            ___nightVision.shadowStrength = 0f;
            ___nightVision.bounceIntensity = 5555f;
            ___nightVision.innerSpotAngle = 999f;
            ___nightVision.spotAngle = 9999f;
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void patchKillPlayer(PlayerControllerB __instance)
        {
            float currentTime = Time.time;

            if (allowKill)
            {
                allowKill = false;
                deathLocation = __instance.transform.position;
                consecutiveDeathExceptions = 0;

                mls.LogMessage("called kill player");
            }
            else
            {
                if (currentTime - lastExceptionTime > exceptionCooldownTime)//reset consecutive deaths when its been too long
                {
                    consecutiveDeathExceptions = 0;
                }

                consecutiveDeathExceptions++;
                lastExceptionTime = currentTime;

                if (consecutiveDeathExceptions >= maxConsecutiveDeathExceptions)
                {
                    mls.LogMessage("Too many consecutive death exceptions. Stuck in death loop.");

                    var tpLocaiton = new Vector3 (0, 10f, 0);
                    __instance.transform.position = tpLocaiton;
                }

                mls.LogMessage("Didn't allow kill, player should be dead on server already");
                throw new Exception("Don't kill player again");
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updatePatch(PlayerControllerB __instance, ref Light ___nightVision)
        {
            if (!allowKill)
            {
                __instance.sprintMeter = 1f;

                if (__instance.isSprinting)
                {
                    FieldInfo sprintMultiplierField = typeof(PlayerControllerB).GetField("sprintMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (sprintMultiplierField != null)
                    {
                        var currentValue = sprintMultiplierField.GetValue(__instance);
                        if (currentValue is float)
                        {
                            var newForce = (float)currentValue * 1.01f;
                            sprintMultiplierField.SetValue(__instance, newForce);
                        }
                        else
                        {
                            mls.LogError("current spritnMultiplier isn't a float?");
                        }
                    }
                    else
                    {
                        mls.LogError("private field not found");
                    }
                }

                if (!isGhostMode)
                {
                    if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)20]).wasPressedThisFrame)//F was pressed
                    {
                        if (__instance.IsOwner && __instance.isPlayerDead && (!__instance.IsServer || __instance.isHostPlayerObject))
                        {
                            mls.LogMessage("attempting to revive");
                            ReviveDeadPlayer(__instance);
                        }
                    }
                }
                else
                {
                    if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)20]).wasPressedThisFrame)//F was pressed
                    {
                        mls.LogMessage("attempt to tp to dead body");
                        __instance.transform.position = __instance.deadBody.transform.position;
                    }
                }

                if (__instance.criticallyInjured == true)
                {
                    __instance.criticallyInjured = false;
                    __instance.bleedingHeavily = false;
                    HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
                }

                if (__instance.playersManager.livingPlayers == 0 || StartOfRound.Instance.shipIsLeaving)
                {
                    //rekill player
                    __instance.DropAllHeldItemsServerRpc();
                    __instance.DisableJetpackControlsLocally();
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
                    __instance.sourcesCausingSinking = 0;
                    __instance.sinkingValue = 0f;
                    __instance.hinderedMultiplier = 1f;
                    __instance.isMovementHindered = 0;
                    __instance.inAnimationWithEnemy = null;
                    HUDManager.Instance.SetNearDepthOfFieldEnabled(enabled: true);
                    HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", value: false);
                    HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
                    StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);

                    //reset my variables
                    allowKill = true;
                    isGhostMode = false;
                    __instance.jumpForce = 13f;
                    __instance.StopAllCoroutines();
                    __instance.nightVision.gameObject.SetActive(false);
                    consecutiveDeathExceptions = 0;

                    FieldInfo isJumpingField = typeof(PlayerControllerB).GetField("isJumping", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo playerSlidingTimerField = typeof(PlayerControllerB).GetField("playerSlidingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo isFallingFromJumpField = typeof(PlayerControllerB).GetField("isFallingFromJump", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (isJumpingField != null && playerSlidingTimerField != null && isFallingFromJumpField != null)
                    {
                        playerSlidingTimerField.SetValue(__instance, 0f);
                        isJumpingField.SetValue(__instance, false);
                        isFallingFromJumpField.SetValue(__instance, false);
                        __instance.fallValue = 0f;
                        __instance.fallValueUncapped = 0f;
                        jumpCoroutine = null;
                    }
                    else
                    {
                        mls.LogError("private fields not found");
                    }
                }

                //toggle night vision
                bool nightVisionFlag = ((Component)___nightVision).gameObject.activeSelf;
                if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)0x10]).wasPressedThisFrame)
                {
                    mls.LogMessage("clicked B, trying to toggle night vision");
                    if (((Component)___nightVision).gameObject.activeSelf)
                    {
                        nightVisionFlag = false;
                    }
                    if (!((Component)___nightVision).gameObject.activeSelf)
                    {
                        nightVisionFlag = true;
                    }
                }
                if (!nightVisionFlag)
                {
                    ((Component)___nightVision).gameObject.SetActive(false);
                }
                if (nightVisionFlag)
                {
                    ((Component)___nightVision).gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPatch("Jump_performed")]
        [HarmonyPostfix]
        static void jump_performedPatch(PlayerControllerB __instance)
        {
            mls.LogMessage($"jump patch hit, allowKill:{allowKill}");
            if (!allowKill)
            {
                FieldInfo isJumpingField = typeof(PlayerControllerB).GetField("isJumping", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo playerSlidingTimerField = typeof(PlayerControllerB).GetField("playerSlidingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo isFallingFromJumpField = typeof(PlayerControllerB).GetField("isFallingFromJump", BindingFlags.NonPublic | BindingFlags.Instance);

                if (isJumpingField != null && playerSlidingTimerField != null && isFallingFromJumpField != null)
                {
                    if (!__instance.quickMenuManager.isMenuOpen && ((__instance.IsOwner && __instance.isPlayerControlled && (!__instance.IsServer || __instance.isHostPlayerObject)) || __instance.isTestingPlayer) && !__instance.inSpecialInteractAnimation && !__instance.isTypingChat && (__instance.isMovementHindered <= 0 || __instance.isUnderwater) && !__instance.isExhausted && (!__instance.isPlayerSliding || (float)playerSlidingTimerField.GetValue(__instance) > 2.5f) && !__instance.isCrouching)
                    {
                        playerSlidingTimerField.SetValue(__instance, 0f);
                        isJumpingField.SetValue(__instance, true);
                        __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - 0.08f, 0f, 1f);
                        __instance.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
                        if (jumpCoroutine != null)
                        {
                            __instance.StopCoroutine(jumpCoroutine);
                        }

                        jumpCoroutine = __instance.StartCoroutine(PlayerJump(__instance, isJumpingField, isFallingFromJumpField));
                    }
                }
                else
                {
                    mls.LogError("private field not found");
                }
                throw new Exception("dont call regular jump method.");
            }
        }

        private static IEnumerator PlayerJump(PlayerControllerB __instance, FieldInfo isJumpingField, FieldInfo isFallingFromJumpField)
        {
            __instance.playerBodyAnimator.SetBool("Jumping", true);
            yield return new WaitForSeconds(0.3f);
            __instance.fallValue = __instance.jumpForce;
            __instance.fallValueUncapped = __instance.jumpForce;
            yield return new WaitForSeconds(0.1f);
            isJumpingField.SetValue(__instance, false);
            isFallingFromJumpField.SetValue(__instance, true);
            yield return new WaitForSeconds(0.1f);
            isFallingFromJumpField.SetValue(__instance, false);
            jumpCoroutine = null;
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPostfix]
        static void damagePlayerPatch(PlayerControllerB __instance)
        {
            if (!allowKill)
            {
                __instance.health = 100;
                __instance.criticallyInjured = false;
                __instance.bleedingHeavily = false;
                HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
            }
        }

        private static void ReviveDeadPlayer(PlayerControllerB __instance)
        {
            mls.LogMessage("add player back server");

            try
            {
                var respawnLocation = getTeleportLocation(__instance);

                var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                var playerIndex = (int)__instance.playerClientId;

                mls.LogMessage($"Reviving player {playerIndex}");

                allPlayerScripts[playerIndex].isSprinting = false;
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

                //increase jump
                __instance.jumpForce = 25f;
                isGhostMode = true;
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }          
        }
    }
}
