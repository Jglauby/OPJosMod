﻿using BepInEx.Logging;
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
using UnityEngine.InputSystem.HID;
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
        private static RaycastHit hit;
        private static int interactableObjectsMask = 832; //this exists in the regular player controller class as a constant

        private static bool isTogglingBrightMode = false;
        private static Coroutine togglingBrightModeCoroutine;
        private static bool isBrightModeOn = false;

        private static LightType OGnightVisionType;
        private static float OGnightVisionIntensity;
        private static float OGnightVisionRange;
        private static float OGnightVisionShadowStrength;
        private static float OGnightVisionBounceIntensity;
        private static float OGnightVisionInnerSpotAngle;
        private static float OGnightVisionSpotAngle;
        private static bool setupValuesYet = false;

        private static float maxSpeed = 10f;

        private static int tpPlayerIndex = 0;
        private static Coroutine tpCoroutine;
        private static bool isTeleporting = false;

        private static bool isTogglingCollisions = false;
        private static Coroutine togglingCollisionsCoroutine;
        private static bool collisionsOn = true;      

        public static float lastInteractedTime = Time.time;

        public static void resetGhostModeVars(PlayerControllerB __instance)
        {
            try
            {
                mls.LogMessage("hit reset ghost vars function");
                allowKill = true;
                isGhostMode = false;
                isTogglingBrightMode = false;
                consecutiveDeathExceptions = 0;
                lastSafeLocations = new Vector3[10];
                timeWhenSafe = Time.time;

                jumpCoroutine = null;
                tpPlayerIndex = 0;
                tpCoroutine = null;
                isTeleporting = false;
                isTogglingCollisions = false;
                isTogglingBrightMode = false;
                isBrightModeOn = false;
                collisionsOn = true;

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

                    setNightVisionMode(__instance, 0);
                    __instance.hasBegunSpectating = false;

                    StartOfRound.Instance.SwitchCamera(GameNetworkManager.Instance.localPlayerController.gameplayCamera);
                    HUDManager.Instance.HideHUD(hide: false);
                    HUDManager.Instance.spectatingPlayerText.text = "";
                    HUDManager.Instance.RemoveSpectateUI();

                    showAliveUI(__instance, true);
                }              
            }
            catch (Exception e)
            {
                mls.LogMessage($"error durign resetign ghost values: {e}");
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
                    savePostGameNotes();
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

        private static void savePostGameNotes()
        {
            ////save regualr stats
            //for (int i = 0; i < RoundManager.Instance.playersManager.allPlayerScripts.Length; i++)
            //{
            //    if (RoundManager.Instance.playersManager.allPlayerScripts[i].playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            //    {
            //        HUDManagerPatch.playerNotes = StartOfRound.Instance.gameStats.allPlayerStats[i]./;
            //        mls.LogMessage($"saved player notes: {HUDManagerPatch.playerNotes.Count}");
            //    }
            //}
            //
            ////save stats for more company
            //for (int i = 0; i < HUDManager.Instance.statsUIElements.playerNotesText.Length; i++)
            //{
            //    var player = RoundManager.Instance.playersManager.allPlayerScripts[i];
            //    if (player.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            //    {
            //        var text = HUDManager.Instance.statsUIElements.playerNotesText[i];
            //
            //        HUDManagerPatch.mcPlayerNotes = text;
            //        mls.LogMessage($"saved MCplayer notes: {HUDManagerPatch.mcPlayerNotes.text}");
            //    }
            //}
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
                    try
                    {
                        if (((ButtonControl)Keyboard.current[ConfigVariables.startGhostModeButton]).wasPressedThisFrame)//P was pressed
                        {
                            mls.LogMessage("attempting to revive");
                            reviveDeadPlayer(__instance);
                        }
                    }
                    catch
                    {
                        //dont fail if no button is selected
                    }
                }
                else //is a ghost
                {
                    listenForGhostHotkeys(__instance);
                }

                //round over reset player vars, and kill ghost
                if (__instance.playersManager.livingPlayers == 0 || StartOfRound.Instance.shipIsLeaving)
                {
                    HUDManager.Instance.DisplayTip("Ship is leaving", "just wait");

                    //rekill player
                    if (isGhostMode)
                    {
                        setToSpectatemode(__instance);
                    }

                    resetGhostModeVars(__instance);
                }

                if (__instance.criticallyInjured == true)
                {
                    __instance.criticallyInjured = false;
                    __instance.bleedingHeavily = false;
                    HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
                }
            }
        }

        private static void listenForGhostHotkeys(PlayerControllerB __instance)
        {
            if (!StartOfRound.Instance.localPlayerController.inTerminalMenu && !StartOfRound.Instance.localPlayerController.isTypingChat)//listen to hotkeys when not typing
            {
                if (!collisionsOn)
                {
                    handleNoClipControls(__instance);
                }

                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.toggleNoClipButton]).wasPressedThisFrame)//Z was pressed
                    {
                        //toggle collisions
                        if (!isTogglingCollisions)
                        {
                            isTogglingCollisions = true;
                            togglingCollisionsCoroutine = __instance.StartCoroutine(toggleCollisions(__instance));
                        }
                    }
                }
                catch { }

                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.teleportBodyButton]).wasPressedThisFrame)//backspace was pressed
                    {
                        mls.LogMessage("attempt to tp to dead body");
                        var tpMessage = "(Teleported to: your dead body)";
                        tpCoroutine = __instance.StartCoroutine(specialTeleportPlayer(__instance, __instance.deadBody.transform.position, tpMessage));
                    }
                }
                catch { }

                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.teleportFrontDoorButton]).wasPressedThisFrame)//up arrow was pressed
                    {
                        mls.LogMessage("attempt to tp to front door");
                        var tpMessage = "(Teleported to: Front Door)";
                        tpCoroutine = __instance.StartCoroutine(specialTeleportPlayer(__instance, RoundManager.FindMainEntrancePosition(true, true), tpMessage));
                    }
                }
                catch { }

                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.teleportShipButton]).wasPressedThisFrame)//down arrow was pressed
                    {
                        mls.LogMessage("attempt to tp to ship");
                        var tpMessage = "(Teleported to: Ship)";
                        tpCoroutine = __instance.StartCoroutine(specialTeleportPlayer(__instance, RoundManager.Instance.playersManager.playerSpawnPositions[0].position, tpMessage));
                    }
                }
                catch { }

                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.switchToSpectateButton]).wasPressedThisFrame)//O was pressed
                    {
                        mls.LogMessage("attempt to switch back to spectate mode");
                        setToSpectatemode(__instance);
                    }
                }
                catch { }

                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.teleportToPlayerBackwardButton]).wasPressedThisFrame)//left was clicked
                    {
                        if (isTeleporting)
                            return;

                        isTeleporting = true;
                        var allPlayers = StartOfRound.Instance.allPlayerScripts;
                        for (int i = 0; i < allPlayers.Length; i++)
                        {
                            tpPlayerIndex = (tpPlayerIndex - 1 + allPlayers.Length) % allPlayers.Length;
                            mls.LogMessage($"tp index:{tpPlayerIndex}");
                            if (!__instance.playersManager.allPlayerScripts[tpPlayerIndex].isPlayerDead
                                && __instance.playersManager.allPlayerScripts[tpPlayerIndex].isPlayerControlled
                                && __instance.playersManager.allPlayerScripts[tpPlayerIndex].playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
                            {
                                var tpMessage = $"(Teleported to:{__instance.playersManager.allPlayerScripts[tpPlayerIndex].playerUsername})";
                                mls.LogMessage($"tp index:{tpPlayerIndex} playerName:{tpMessage}");
                                tpCoroutine = __instance.StartCoroutine(specialTeleportPlayer(__instance, __instance.playersManager.allPlayerScripts[tpPlayerIndex].transform.position, tpMessage));
                                return;
                            }
                        }
                    }
                } catch { }

                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.teleportToPlayerForwardButton]).wasPressedThisFrame)//right was clicked
                    {
                        if (isTeleporting)
                            return;

                        isTeleporting = true;
                        var allPlayers = StartOfRound.Instance.allPlayerScripts;
                        for (int i = 0; i < allPlayers.Length; i++)
                        {
                            tpPlayerIndex = (tpPlayerIndex + 1) % allPlayers.Length;
                            mls.LogMessage($"tp index:{tpPlayerIndex}");
                            if (!__instance.playersManager.allPlayerScripts[tpPlayerIndex].isPlayerDead
                                && __instance.playersManager.allPlayerScripts[tpPlayerIndex].isPlayerControlled
                                && __instance.playersManager.allPlayerScripts[tpPlayerIndex].playerClientId != StartOfRound.Instance.localPlayerController.playerClientId)
                            {
                                var tpMessage = $"(Teleported to:{__instance.playersManager.allPlayerScripts[tpPlayerIndex].playerUsername})";
                                mls.LogMessage($"tp index:{tpPlayerIndex} playerName:{tpMessage}");
                                tpCoroutine = __instance.StartCoroutine(specialTeleportPlayer(__instance, __instance.playersManager.allPlayerScripts[tpPlayerIndex].transform.position, tpMessage));
                                return;
                            }
                        }
                    }
                }
                catch { }

                //toggle night vision
                try
                {
                    if (((ButtonControl)Keyboard.current[ConfigVariables.toggleBrightModeButton]).wasPressedThisFrame)
                    {
                        if (!isTogglingBrightMode)
                        {
                            isTogglingBrightMode = true;
                            togglingBrightModeCoroutine = __instance.StartCoroutine(toggleBrightMode(__instance));
                        }
                    }
                }
                catch { }
            }
        }

        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        private static bool interact_performedPatch(PlayerControllerB __instance)
        {
            if (!isGhostMode)
                return true;

            if (__instance.IsOwner && !__instance.isPlayerDead && (!__instance.IsServer || __instance.isHostPlayerObject))
            {
                if (!canUse(__instance))               
                    return false;               
                    
                if (shouldHaveDelay(__instance))
                {
                    if (Time.time - lastInteractedTime > ConfigVariables.waitTimeBetweenInteractions)
                    {
                        lastInteractedTime = Time.time;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool shouldHaveDelay(PlayerControllerB __instance, bool showDebug = true)
        {
            if (!isGhostMode || ConfigVariables.OPness == OPnessModes.Unrestricted)
                return false;

            if (__instance.hoveringOverTrigger != null && __instance.hoveringOverTrigger.gameObject != null)
            {
                var objectName = GeneralUtils.simplifyObjectNames(__instance.hoveringOverTrigger.gameObject.name);

                if(showDebug)
                    mls.LogMessage($"tried to interact with: {objectName}");

                /**
                 * Cube = Cabinent door, regular door, ship horn, move items in and out of cabnent, camera switch, open and close ship door
                 * LadderTrigger = ladder
                 * EntranceTeleportA = main door
                 * StartGameLever = ship lever
                 * TerminalScript = terminal
                 * ButtonGlass = glass on teleport button
                 * RedButton = either teleport button
                 * Trigger = battery charger
                 */
                string[] noDelayObjects = { 
                    "Cube", "EntranceTeleportA" , "StartGameLever", "TerminalScript", "ButtonGlass", "Trigger" 
                };

                if (noDelayObjects.Contains(objectName))
                    return false;
            }
            else
            {
                if(showDebug)
                    mls.LogMessage("faliled to find interacted with name");
            }


            return true;
        }

        private static bool canUse(PlayerControllerB __instance)
        {
            if (!isGhostMode || ConfigVariables.OPness == OPnessModes.Unrestricted)
                return true;

            if (ConfigVariables.OPness == OPnessModes.Limited)
                return false;

            string[] nonoObjects = {
                    "RedButton", "LadderTrigger", "RagdollGrabbableObject"
                };

            //check interactions
            if (__instance.hoveringOverTrigger != null && __instance.hoveringOverTrigger.gameObject != null)
            {
                var objectName = GeneralUtils.simplifyObjectNames(__instance.hoveringOverTrigger.gameObject.name);               

                if (nonoObjects.Contains(objectName))
                    return false;
            }

            //check grabbing objects
            Ray interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
            Physics.Raycast(interactRay, out RaycastHit hit, __instance.grabDistance, interactableObjectsMask);
            GrabbableObject currentlyGrabbingObject = hit.collider?.GetComponent<GrabbableObject>();        
            if (currentlyGrabbingObject != null)
            {
                var objectName = GeneralUtils.simplifyObjectNames(currentlyGrabbingObject.name);

                if (nonoObjects.Contains(objectName) || ConfigVariables.canPickupScrap == false)
                    return false;
            }

            return true;
        }

        [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
        [HarmonyPostfix]
        private static void setHoverTipAndCurrentInteractTriggerPatch(PlayerControllerB __instance)
        {
            if (!isGhostMode)
                return;

            if (shouldHaveDelay(__instance, false))
            {
                var lastITime = lastInteractedTime;
                var waitTime = ConfigVariables.waitTimeBetweenInteractions;
                var remainingTime = waitTime - (Time.time - lastITime);

                if (Time.time - lastITime <= waitTime && __instance.cursorTip.text != "")
                {
                    __instance.cursorTip.text = $"Wait: {(int)remainingTime}";
                }
            }

            if (!canUse(__instance) && __instance.cursorTip.text != "")
            {
                __instance.cursorTip.text = "Can't use as a ghost!";
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

        private static void handleNoClipControls(PlayerControllerB __instance)
        {
            try
            {
                if (((ButtonControl)Keyboard.current[ConfigVariables.noClipForwardButton]).isPressed)//W is pressed
                {
                    var currentRotation = __instance.transform.rotation;

                    Vector3 moveDirection = currentRotation * Vector3.forward;
                    moveDirection.Normalize();

                    __instance.transform.position += moveDirection * ConfigVariables.noClipSpeed;
                }
            } catch { }

            try
            {
                if (((ButtonControl)Keyboard.current[ConfigVariables.noClipLeftButton]).isPressed)//A is pressed
                {
                    var currentRotation = __instance.transform.rotation;

                    Quaternion leftRotation = Quaternion.AngleAxis(-90f, Vector3.up);
                    Vector3 leftDirection = leftRotation * currentRotation * Vector3.forward;
                    leftDirection.Normalize();

                    __instance.transform.position += leftDirection * ConfigVariables.noClipSpeed;
                }
            }
            catch { }

            try
            {
                if (((ButtonControl)Keyboard.current[ConfigVariables.noClipRightButton]).isPressed)//D is pressed
                {
                    var currentRotation = __instance.transform.rotation;

                    Quaternion rightRotation = Quaternion.AngleAxis(90f, Vector3.up);
                    Vector3 rightDirection = rightRotation * currentRotation * Vector3.forward;
                    rightDirection.Normalize();

                    __instance.transform.position += rightDirection * ConfigVariables.noClipSpeed;
                }
            }
            catch { }

            try
            {
                if (((ButtonControl)Keyboard.current[ConfigVariables.noClipBackwardButton]).isPressed)//S is pressed
                {
                    var currentRotation = __instance.transform.rotation;

                    Vector3 backwardDirection = currentRotation * Vector3.back;
                    backwardDirection.Normalize();

                    __instance.transform.position += backwardDirection * ConfigVariables.noClipSpeed;
                }
            }
            catch { }

            try
            {
                if (((ButtonControl)Keyboard.current[ConfigVariables.noClipUpButton]).isPressed)//space is pressed
                {
                    Vector3 upDirection = Vector3.up;

                    __instance.transform.position += upDirection * ConfigVariables.noClipSpeed;
                }
            }
            catch { }

            try
            {
                if (((ButtonControl)Keyboard.current[ConfigVariables.noClipDownButton]).isPressed)//left shift is pressed
                {
                    Vector3 upDirection = -Vector3.up;

                    __instance.transform.position += upDirection * ConfigVariables.noClipSpeed;
                }
            }
            catch { }
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

                showAliveUI(playerControllerB, false);

                //reset move speed
                FieldInfo sprintMultiplierField = typeof(PlayerControllerB).GetField("sprintMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
                if (sprintMultiplierField != null )
                {
                    sprintMultiplierField.SetValue(playerControllerB, 1f);
                }

                //reset dead players voices and icons manually on revive
                HUDManagerPatch.updateBoxesSpectateUI(HUDManager.Instance);

                //make ghostsUnTarget you
                if (!ConfigVariables.enemiesDetectYou)
                    EnemyAIPatch.makeEnemiesDropFocus(__instance);
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }

        public static void rekillPlayerLocally(PlayerControllerB __instance)
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
        }

        public static void setToSpectatemode(PlayerControllerB __instance)
        {
            __instance = StartOfRound.Instance.localPlayerController;

            showAliveUI(__instance, false);
            rekillPlayerLocally(__instance);
            __instance.hasBegunSpectating = true;
            HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
            isGhostMode = false;

            ChangeAudioListenerToObject(__instance, __instance.playersManager.spectateCamera.gameObject);

            if(isBrightModeOn)
                togglingBrightModeCoroutine = __instance.StartCoroutine(toggleBrightMode(__instance));
        }         

        private static IEnumerator specialTeleportPlayer(PlayerControllerB __instance, Vector3 newPos, string message)
        {
            if (tpCoroutine != null)
            {
                __instance.StopCoroutine(tpCoroutine);
            }

            HUDManager.Instance.spectatingPlayerText.text = message;
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(newPos);
            yield return new WaitForSeconds(1.5f);

            HUDManager.Instance.spectatingPlayerText.text = "";     
            isTeleporting = false;
        }

        private static IEnumerator toggleCollisions(PlayerControllerB __instance)
        {
            if (togglingCollisionsCoroutine != null)
            {
                __instance.StopCoroutine(togglingCollisionsCoroutine);
            }

            __instance = StartOfRound.Instance.localPlayerController;
            yield return new WaitForSeconds(0.1f);

            if (collisionsOn)
            {
                mls.LogMessage("collisions are off");
                HUDManager.Instance.DisplayTip("NoClip", "On");
                collisionsOn = false;

                __instance.playerCollider.enabled = false;
            }
            else
            {
                mls.LogMessage("collisions are on");
                HUDManager.Instance.DisplayTip("NoClip", "Off");
                collisionsOn = true;

                __instance.playerCollider.enabled = true;
            }

            isTogglingCollisions = false;
        }

        private static IEnumerator toggleBrightMode(PlayerControllerB __instance)
        {
            if (togglingBrightModeCoroutine != null)
            {
                __instance.StopCoroutine(togglingBrightModeCoroutine);
            }

            __instance = StartOfRound.Instance.localPlayerController;
            yield return new WaitForSeconds(0.1f);

            if (isBrightModeOn)
            {
                mls.LogMessage("turn off bright mode");
                isBrightModeOn = false;
                setNightVisionMode(__instance, 0);
                __instance.isInsideFactory = false;
                ((Component)__instance.nightVision).gameObject.SetActive(false);
            }
            else
            {
                mls.LogMessage("turn on bright mode");
                isBrightModeOn = true;
                setNightVisionMode(__instance, 1);
                __instance.isInsideFactory = true;
                ((Component)__instance.nightVision).gameObject.SetActive(true);
            }

            isTogglingBrightMode = false;
        }
    }
}
