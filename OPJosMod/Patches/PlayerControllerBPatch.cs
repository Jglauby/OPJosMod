using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.Utils;
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
        public static bool isGhostMode = false;
        private static Coroutine jumpCoroutine;

        private static Vector3 deathLocation;
        private static int consecutiveDeathExceptions = 0;
        private static int maxConsecutiveDeathExceptions = 3;
        private static float exceptionCooldownTime = 2f;
        private static float lastExceptionTime = 0f;

        private static Vector3[] lastSafeLocations = new Vector3[10];
        private static int safeLocationsIndex = 0;
        private static float timeWhenSafe = Time.time;

        private static Ray interactRay;
        private static bool nightVisionFlag = false;

        private static LightType OGnightVisionType;
        private static float OGnightVisionIntensity;
        private static float OGnightVisionRange;
        private static float OGnightVisionShadowStrength;
        private static float OGnightVisionBounceIntensity;
        private static float OGnightVisionInnerSpotAngle;
        private static float OGnightVisionSpotAngle;
        private static bool setupValuesYet = false;

        private static float maxSpeed = 10f;

        public static void resetGhostModeVars(PlayerControllerB __instance)
        {
            try
            {
                if (__instance != null) 
                {
                    __instance.StopAllCoroutines();
                    if (__instance.nightVision != null)
                        ((Component)__instance.nightVision).gameObject.SetActive(true);

                    FieldInfo isJumpingField = typeof(PlayerControllerB).GetField("isJumping", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo playerSlidingTimerField = typeof(PlayerControllerB).GetField("playerSlidingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo isFallingFromJumpField = typeof(PlayerControllerB).GetField("isFallingFromJump", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo sprintMultiplierField = typeof(PlayerControllerB).GetField("sprintMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (isJumpingField != null && playerSlidingTimerField != null && isFallingFromJumpField != null && sprintMultiplierField != null)
                    {
                        playerSlidingTimerField.SetValue(__instance, 0f);
                        isJumpingField.SetValue(__instance, false);
                        isFallingFromJumpField.SetValue(__instance, false);
                        __instance.fallValue = 0f;
                        __instance.fallValueUncapped = 0f;
                        sprintMultiplierField.SetValue(__instance, 1f);
                    }
                    else
                    {
                        mls.LogError("private fields not found");
                    }

                    setupValuesYet = false;
                    setNightVisionMode(__instance, 0);
                    __instance.hasBegunSpectating = false;

                    StartOfRound.Instance.SwitchCamera(GameNetworkManager.Instance.localPlayerController.gameplayCamera);
                    HUDManager.Instance.HideHUD(hide: false);
                    HUDManager.Instance.spectatingPlayerText.text = "";
                    HUDManager.Instance.RemoveSpectateUI();

                    showAliveUI(__instance, true);
                }
                
                mls.LogMessage("hit reset ghost vars function");
                allowKill = true;
                isGhostMode = false;                
                nightVisionFlag = false;
                consecutiveDeathExceptions = 0;
                lastSafeLocations = new Vector3[10];
                timeWhenSafe = Time.time;

                jumpCoroutine = null;
          
            }
            catch (Exception e)
            {
                mls.LogMessage(e);
            }
        }

        //mode: 0 => regular, 1 => super bright
        private static void setNightVisionMode(PlayerControllerB __instance, int mode)
        {
            if (mode == 0)
            {
                mls.LogMessage("setting default night vision values");
                __instance.nightVision.type = OGnightVisionType;
                __instance.nightVision.intensity = OGnightVisionIntensity;
                __instance.nightVision.range = OGnightVisionRange;
                __instance.nightVision.shadowStrength = OGnightVisionShadowStrength;
                __instance.nightVision.bounceIntensity = OGnightVisionBounceIntensity;
                __instance.nightVision.innerSpotAngle = OGnightVisionInnerSpotAngle;
                __instance.nightVision.spotAngle = OGnightVisionSpotAngle;
            }
            else if (mode == 1)
            {
                __instance.nightVision.type = (LightType)2;
                __instance.nightVision.intensity = 44444f;
                __instance.nightVision.range = 99999f;
                __instance.nightVision.shadowStrength = 0f;
                __instance.nightVision.bounceIntensity = 5555f;
                __instance.nightVision.innerSpotAngle = 999f;
                __instance.nightVision.spotAngle = 9999f;
            }
        }

        private static void showAliveUI(PlayerControllerB __instance, bool show)
        {
            HUDManager.Instance.Clock.canvasGroup.gameObject.SetActive(show);            
            HUDManager.Instance.selfRedCanvasGroup.gameObject.SetActive(show);
            __instance.sprintMeterUI.gameObject.SetActive(show);
            HUDManager.Instance.weightCounter.gameObject.SetActive(show);

            if (!show)
            {
                foreach (var temp in HUDManager.Instance.controlTipLines)
                {
                    temp.text = "";
                }
            }
        }

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

        private static void ChangeAudioListenerToObject(PlayerControllerB __instance, GameObject addToObject)
        {
            __instance.activeAudioListener.transform.SetParent(addToObject.transform);
            __instance.activeAudioListener.transform.localEulerAngles = Vector3.zero;
            __instance.activeAudioListener.transform.localPosition = Vector3.zero;
            StartOfRound.Instance.audioListener = __instance.activeAudioListener;
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static bool patchKillPlayer(PlayerControllerB __instance)
        {
            float currentTime = Time.time;

            if (__instance.IsOwner)
            {
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

                        Vector3 lastSafeLocation = lastSafeLocations[(safeLocationsIndex - 9 + lastSafeLocations.Length) % lastSafeLocations.Length];
                        __instance.transform.position = lastSafeLocation;
                    }

                    mls.LogMessage("Didn't allow kill, player should be dead on server already");
                    return false;
                }
            }

            mls.LogMessage("called kill player as __instance.playerClientId != StartOfRoundLocalPlayerClientID");
            return true;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updatePatch(PlayerControllerB __instance, ref Light ___nightVision)
        {
            if(setupValuesYet == false && allowKill)
            {
                mls.LogMessage("setting default night vision values");
                setupValuesYet = true;
                OGnightVisionType = __instance.nightVision.type;
                OGnightVisionIntensity = __instance.nightVision.intensity;
                OGnightVisionRange = __instance.nightVision.range;
                OGnightVisionShadowStrength = __instance.nightVision.shadowStrength;
                OGnightVisionBounceIntensity = __instance.nightVision.bounceIntensity;
                OGnightVisionInnerSpotAngle = __instance.nightVision.innerSpotAngle;
                OGnightVisionSpotAngle = __instance.nightVision.spotAngle;
            }

            if ((Time.time - timeWhenSafe) >= 1.0f)
            {
                lastSafeLocations[safeLocationsIndex] = __instance.transform.position;
                safeLocationsIndex = (safeLocationsIndex + 1) % lastSafeLocations.Length;
                timeWhenSafe = Time.time;
            }

            //mls.LogMessage($"update running, allowKill: {allowKill}, isGhostMode: {isGhostMode}");
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
                            if ((float)currentValue < maxSpeed)
                            {
                                var newForce = (float)currentValue * 1.015f;
                                sprintMultiplierField.SetValue(__instance, newForce);
                            }
                            else
                            {
                                //mls.LogMessage("max speed hit");
                            }
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
                    if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)30]).wasPressedThisFrame)//P was pressed
                    {
                        mls.LogMessage("attempting to revive");
                        reviveDeadPlayer(__instance);                        
                    }
                }
                else //is a ghost
                {
                    if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)17]).wasPressedThisFrame && !__instance.inTerminalMenu)//C was pressed
                    {
                        mls.LogMessage("attempt to tp to dead body");
                        __instance.transform.position = __instance.deadBody.transform.position;
                    }

                    if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)0x20]).wasPressedThisFrame && !__instance.inTerminalMenu)//R was pressed
                    {
                        mls.LogMessage("attempt to tp to front door");
                        __instance.transform.position = RoundManager.FindMainEntrancePosition(true, true);
                    }

                    if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)29]).wasPressedThisFrame && !__instance.inTerminalMenu)//O was pressed
                    {
                        mls.LogMessage("attempt to switch back to spectate mode");
                        setToSpectatemode(__instance);
                    }
                }

                //round over reset player vars, and kill ghost
                if (__instance.playersManager.livingPlayers == 0 || StartOfRound.Instance.shipIsLeaving)
                {
                    HUDManager.Instance.DisplayTip("Ship is leaving", "just wait");

                    //rekill player
                    if (isGhostMode)
                    {
                        rekillPlayerLocally(__instance, true);
                    }

                    resetGhostModeVars(__instance);
                }

                if (__instance.criticallyInjured == true)
                {
                    __instance.criticallyInjured = false;
                    __instance.bleedingHeavily = false;
                    HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
                }

                //toggle night vision
                if (((ButtonControl)Keyboard.current[(UnityEngine.InputSystem.Key)0x10]).wasPressedThisFrame && !__instance.inTerminalMenu)
                {
                    mls.LogMessage("clicked B, trying to toggle night vision");
                    if (((Component)___nightVision).gameObject.activeSelf)
                    {
                        setNightVisionMode(__instance, 0);
                        __instance.isInsideFactory = false;
                        nightVisionFlag = false;
                    }
                    if (!((Component)___nightVision).gameObject.activeSelf)
                    {
                        setNightVisionMode(__instance, 1);
                        __instance.isInsideFactory = true;
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

        //a playercontrollerb private function manually written out
        private static bool IsPlayerNearGround(PlayerControllerB __instance)
        {
            interactRay = new Ray(__instance.transform.position, Vector3.down);
            return Physics.Raycast(interactRay, 0.15f, StartOfRound.Instance.allPlayersCollideWithMask, QueryTriggerInteraction.Ignore);
        }

        //a playercontrollerb private function manualy written out
        private static void PlayerHitGroundEffects(PlayerControllerB __instance)
        {
            __instance.GetCurrentMaterialStandingOn();
            if (__instance.fallValue < -9f)
            {
                if (__instance.fallValue < -16f)
                {
                    __instance.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundHard, 1f);
                    WalkieTalkie.TransmitOneShotAudio(__instance.movementAudio, StartOfRound.Instance.playerHitGroundHard);
                }
                else if (__instance.fallValue < -2f)
                {
                    __instance.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundSoft, 1f);
                }

                __instance.LandFromJumpServerRpc(__instance.fallValue < -16f);
            }

            if (__instance.takingFallDamage && !__instance.jetpackControls && !__instance.disablingJetpackControls && !__instance.isSpeedCheating && allowKill)
            {
                Debug.Log($"Fall damage: {__instance.fallValueUncapped}");
                if (__instance.fallValueUncapped < -48.5f)
                {
                    __instance.DamagePlayer(100, hasDamageSFX: true, callRPC: true, CauseOfDeath.Gravity);
                }
                else if (__instance.fallValueUncapped < -45f)
                {
                    __instance.DamagePlayer(80, hasDamageSFX: true, callRPC: true, CauseOfDeath.Gravity);
                }
                else if (__instance.fallValueUncapped < -40f)
                {
                    __instance.DamagePlayer(50, hasDamageSFX: true, callRPC: true, CauseOfDeath.Gravity);
                }
                else
                {
                    __instance.DamagePlayer(30, hasDamageSFX: true, callRPC: true, CauseOfDeath.Gravity);
                }
            }

            if (__instance.fallValue < -16f)
            {
                RoundManager.Instance.PlayAudibleNoise(__instance.transform.position, 7f);
            }
        }

        [HarmonyPatch("Jump_performed")]
        [HarmonyPrefix]
        static void jump_performedPatch(PlayerControllerB __instance)
        {
            //mls.LogMessage($"jump performed, jumpForce:{__instance.jumpForce}, allowKill:{allowKill}");

            FieldInfo isJumpingField = typeof(PlayerControllerB).GetField("isJumping", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo playerSlidingTimerField = typeof(PlayerControllerB).GetField("playerSlidingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo isFallingFromJumpField = typeof(PlayerControllerB).GetField("isFallingFromJump", BindingFlags.NonPublic | BindingFlags.Instance);

            if (isJumpingField != null && playerSlidingTimerField != null && isFallingFromJumpField != null)
            {
                if (!__instance.quickMenuManager.isMenuOpen && ((__instance.IsOwner && __instance.isPlayerControlled && (!__instance.IsServer || __instance.isHostPlayerObject)) || __instance.isTestingPlayer) && !__instance.inSpecialInteractAnimation && !__instance.isTypingChat && (__instance.isMovementHindered <= 0 || __instance.isUnderwater) && !__instance.isExhausted && (!__instance.isPlayerSliding || (float)playerSlidingTimerField.GetValue(__instance) > 2.5f) && !__instance.isCrouching)
                {
                    //if not dead/ghost then you need to check if youre in the air to allow jump
                    if (!allowKill || ((__instance.thisController.isGrounded || (!(bool)isJumpingField.GetValue(__instance) && IsPlayerNearGround(__instance))) && !(bool)isJumpingField.GetValue(__instance)))
                    {
                        playerSlidingTimerField.SetValue(__instance, 0f);
                        isJumpingField.SetValue(__instance, true);
                        __instance.sprintMeter = Mathf.Clamp(__instance.sprintMeter - 0.08f, 0f, 1f);
                        //__instance.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
                        if (jumpCoroutine != null)
                        {
                            __instance.StopCoroutine(jumpCoroutine);
                        }

                        jumpCoroutine = __instance.StartCoroutine(PlayerJump(__instance, isJumpingField, isFallingFromJumpField));
                    }
                }
            }
            else
            {
                mls.LogError("private field not found");
            }
        }

        private static IEnumerator PlayerJump(PlayerControllerB __instance, FieldInfo isJumpingField, FieldInfo isFallingFromJumpField)
        {
            if (allowKill)
                __instance.jumpForce = 13f;
            else
                __instance.jumpForce = 25f;

            __instance.playerBodyAnimator.SetBool("Jumping", true);
            yield return new WaitForSeconds(0.15f);
            __instance.fallValue = __instance.jumpForce;
            __instance.fallValueUncapped = __instance.jumpForce;
            yield return new WaitForSeconds(0.1f);
            isJumpingField.SetValue(__instance, false);
            isFallingFromJumpField.SetValue(__instance, true);

            if (!allowKill)
                yield return new WaitForSeconds(0.1f);
            else
                yield return new WaitUntil(() => __instance.thisController.isGrounded);

            __instance.playerBodyAnimator.SetBool("Jumping", value: false);
            isFallingFromJumpField.SetValue(__instance, false);
            PlayerHitGroundEffects(__instance);
            jumpCoroutine = null;
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        static void damagePlayerPatch(PlayerControllerB __instance, ref int damageNumber)
        {
            if (!allowKill)
            {
                __instance.health = 100;
                __instance.criticallyInjured = false;
                __instance.bleedingHeavily = false;

                var healthToAdd = 100 - damageNumber;
                __instance.DamagePlayerServerRpc(-healthToAdd, __instance.health);
            }
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPostfix]
        static void damagePlayerPostPatch(PlayerControllerB __instance, ref int damageNumber)
        {
            if (!allowKill)
            {
                HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
            }
        }

        private static void reviveDeadPlayer(PlayerControllerB __instance)
        {
            try
            {
                var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                var playerIndex = StartOfRound.Instance.localPlayerController.playerClientId;

                var respawnLocation = getTeleportLocation(allPlayerScripts[playerIndex]);

                mls.LogMessage($"Reviving player {playerIndex}");

                allPlayerScripts[playerIndex].velocityLastFrame = new Vector3(0, 0, 0);

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
                    //allPlayerScripts[playerIndex].TeleportPlayer(respawnLocation);
                    allPlayerScripts[playerIndex].transform.position = respawnLocation;
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
                SoundManager.Instance.SetPlayerPitch(1f, (int)playerIndex);

                PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
                playerControllerB.bleedingHeavily = false;
                playerControllerB.criticallyInjured = false;
                playerControllerB.playerBodyAnimator.SetBool("Limp", value: false);
                playerControllerB.health = 100;
                HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
                playerControllerB.spectatedPlayerScript = null;
                HUDManager.Instance.audioListenerLowPass.enabled = false;
                StartOfRound.Instance.SetSpectateCameraToGameOverMode(enableGameOver: false, playerControllerB);

                isGhostMode = true;
                HUDManager.Instance.spectatingPlayerText.text = "";
                HUDManager.Instance.holdButtonToEndGameEarlyText.text = "";
                HUDManager.Instance.holdButtonToEndGameEarlyMeter.gameObject.SetActive(false);
                HUDManager.Instance.holdButtonToEndGameEarlyVotesText.text = "";
                setupValuesYet = false;
                setNightVisionMode(playerControllerB, 0);
                nightVisionFlag = false;

                showAliveUI(playerControllerB, false);

                //reset move speed
                FieldInfo sprintMultiplierField = typeof(PlayerControllerB).GetField("sprintMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
                if (sprintMultiplierField != null )
                {
                    sprintMultiplierField.SetValue(playerControllerB, 1f);
                }

                //reset dead players voices and icons manually on revive
                HUDManagerPatch.updateBoxesSpectateUI(HUDManager.Instance);
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }

        public static void rekillPlayerLocally(PlayerControllerB __instance, bool gameOver)
        {
            mls.LogMessage("try to rekill player locally");
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

            StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);

            if (gameOver)
                HUDManager.Instance.DisplayTip("Ship is leaving", "just wait");
        }

        private static void setToSpectatemode(PlayerControllerB __instance)
        {
            __instance = StartOfRound.Instance.localPlayerController;

            showAliveUI(__instance, false);
            rekillPlayerLocally(__instance, false);
            __instance.hasBegunSpectating = true;
            HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
            isGhostMode = false;

            ChangeAudioListenerToObject(__instance, __instance.playersManager.spectateCamera.gameObject);
        }         
    }
}
