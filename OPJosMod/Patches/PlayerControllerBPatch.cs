using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.GodMode.Enums;
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

        private static bool allowKill = false;

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void patchKillPlayer(PlayerControllerB __instance, ref int deathAnimation, ref bool spawnBody, ref Vector3 bodyVelocity, ref CauseOfDeath causeOfDeath)
        {
            if (allowKill)
            {
                mls.LogMessage("allowed regualr kill process. as allowKill was true");
            }
            else
            {
                mls.LogMessage("do patch kill as custom property (allowKill) is set to false");
                doPatchKill(__instance, deathAnimation, spawnBody, bodyVelocity, causeOfDeath);
            }
        }

        private static void doPatchKill(PlayerControllerB __instance, int deathAnimation, bool spawnBody, Vector3 bodyVelocity, CauseOfDeath causeOfDeath)
        {
            mls.LogMessage("died kinda");
            FieldInfo wasUnderWaterLastFrameField = typeof(PlayerControllerB).GetField("wasUnderwaterLastFrame", BindingFlags.NonPublic | BindingFlags.Instance);

            if (wasUnderWaterLastFrameField != null)
            {
                Vector3 deathLocation = __instance.transform.position;

                if (__instance.IsOwner && !__instance.isPlayerDead && __instance.AllowPlayerDeath())
                {
                    __instance.DropAllHeldItemsAndSync();
                    __instance.transform.localPosition = new Vector3(0, -75, 0);
                    __instance.StartCoroutine(fakeKillPlayer(__instance, 0.25f, deathAnimation, spawnBody, bodyVelocity, causeOfDeath, deathLocation, wasUnderWaterLastFrameField));
                }
            }
            else
            {
                mls.LogError("private field was not found in patch kill player");
            }

            throw new Exception("actually don't kill");
        }

        private static IEnumerator fakeKillPlayer(PlayerControllerB __instance, float time, int deathAnimation, bool spawnBody,
            Vector3 bodyVelocity, CauseOfDeath causeOfDeath, Vector3 deathLocation, FieldInfo wasUnderWaterLastFrameField)
        {
            mls.LogMessage("Before delay - Time: " + Time.time);
            yield return new WaitForSeconds(time);
            mls.LogMessage("After delay - Time: " + Time.time);

            __instance.isPlayerDead = true;
            __instance.isPlayerControlled = false;
            __instance.thisPlayerModelArms.enabled = false;
            __instance.localVisor.position = StartOfRound.Instance.notSpawnedPosition.position;
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
            ChangeAudioListenerToObject(__instance, StartOfRound.Instance.spectateCamera.gameObject);
            SoundManager.Instance.SetDiageticMixerSnapshot();
            HUDManager.Instance.SetNearDepthOfFieldEnabled(enabled: true);
            HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", value: false);
            HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
            HUDManager.Instance.HideHUD(hide: true);
            StopHoldInteractionOnTrigger(__instance);

            if (spawnBody)
            {
                //instead of spawning dead body, spawn and instant kill mimic body of my name via server sends. 
                PlayerControllerB dumbyScript = StartOfRound.Instance.allPlayerScripts[2];
                mls.LogMessage($"try to connect dumby player to server, dumbyScriptPLayerClientID:{dumbyScript.playerClientId} at: {Time.time}");
                StartOfRound.Instance.OnClientConnect(dumbyScript.playerClientId);//might not need if the update location ends up working               

                //enable the dumby player locally
                //dumbyScript.playersManager.shipDoorsEnabled = false;
                dumbyScript.isPlayerDead = false;
                dumbyScript.isPlayerControlled = true;
                dumbyScript.thisPlayerModelArms.enabled = true;
                dumbyScript.localVisor.position = dumbyScript.transform.position;
                dumbyScript.DisablePlayerModel(dumbyScript.gameObject, true);

                //teleport player, should sync locaiton since its active
                yield return new WaitForSeconds(time);
                mls.LogMessage($"teleport connected player at: {Time.time}");
                deathLocation.y = deathLocation.y + 1f;
                dumbyScript.transform.position = deathLocation;

                //call funciton to update its locaiton on server
                //
                //this doesn't work as "[Error  : Unity Log] Only the owner can invoke a ServerRpc that requires ownership!"
                MethodInfo methodInfo = typeof(PlayerControllerB).GetMethod("UpdatePlayerPositionServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
                if (methodInfo != null)
                {
                    //Vector3 newPos, bool inElevator, bool inShipRoom, bool exhausted, bool isPlayerGrounded
                    object[] parameters = new object[] {
                        deathLocation,
                        __instance.isInElevator,
                        __instance.isInHangarShipRoom,
                        __instance.isExhausted,
                        __instance.isGroundedOnServer
                    };

                    methodInfo.Invoke(dumbyScript, parameters);
                }

                //kill spawned player
                yield return new WaitForSeconds(time);
                mls.LogMessage($"kill spawned player, time:{Time.time}");
                allowKill = true;
                //dumbyScript.KillPlayer(bodyVelocity, spawnBody, causeOfDeath, deathAnimation);
                allowKill = false;
            }

            StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);
            __instance.isInGameOverAnimation = 1.5f;
            __instance.cursorTip.text = "";
            __instance.cursorIcon.enabled = false;
            __instance.DisableJetpackControlsLocally();
        }     

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPostfix]
        static void patchDamagePlayer(PlayerControllerB __instance)
        {
            //__instance.health = 100;
            //HUDManager.Instance.UpdateHealthUI(__instance.health, false);
        }

        [HarmonyPatch("UpdatePlayerPositionServerRpc")]
        [HarmonyPostfix]
        static void patchUpdatePlayerPositionServerRpc(PlayerControllerB __instance)
        {
            //mls.LogMessage("hit UpdatePlayerPositionServerRpc() function");
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        static void tempPatch(PlayerControllerB __instance)
        {
            __instance.health = 0;
            HUDManager.Instance.UpdateHealthUI(__instance.health, false);
        }

        [HarmonyPatch("Crouch")]
        [HarmonyPrefix]
        static void patchCrouch(PlayerControllerB __instance)
        {
            //mls.LogMessage("clicked crouch, try to teleport my location");
            //__instance.transform.localPosition = new Vector3(0, -100, 0);
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

        //slightly modified version of ReviveDeadPlayers from the StartOfRound object
        private static void ReviveDeadPlayer(PlayerControllerB __instance)
        {
            try
            {
                //need to make this location of fake dead body, if possible
                Vector3 respawnLocation = new Vector3(0, 0, 0);

                var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                var playerIndex = (int)__instance.playerClientId;
                //
                StartOfRound.Instance.allPlayersDead = false;

                if (playerIndex < 0 || playerIndex >= allPlayerScripts.Length)
                {
                    mls.LogError("Invalid player index for revival.");
                    return;
                }

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

                    allPlayerScripts[playerIndex].mapRadarDotAnimator.SetBool("dead", value: false);

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
                RagdollGrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>();
                for (int j = 0; j < array.Length; j++)
                {
                    if (!array[j].isHeld)
                    {
                        if (StartOfRound.Instance.IsServer)
                        {
                            if (array[j].NetworkObject.IsSpawned)
                            {
                                array[j].NetworkObject.Despawn();
                            }
                            else
                            {
                                UnityEngine.Object.Destroy(array[j].gameObject);
                            }
                        }
                    }
                    else if (array[j].isHeld && array[j].playerHeldBy != null)
                    {
                        array[j].playerHeldBy.DropAllHeldItems();
                    }
                }
                DeadBodyInfo[] array2 = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>();
                for (int k = 0; k < array2.Length; k++)
                {
                    UnityEngine.Object.Destroy(array2[k].gameObject);
                }
                StartOfRound.Instance.livingPlayers = StartOfRound.Instance.connectedPlayersAmount + 1;
                StartOfRound.Instance.allPlayersDead = false;
                StartOfRound.Instance.UpdatePlayerVoiceEffects();

                //__instance.playersManager.shipAnimator.ResetTrigger("ShipLeave")
                HUDManager.Instance.HideHUD(hide: false);

                //decrease dead player amount across all clients
                //StartOfRound.Instance.PlayerHasRevivedServerRpc();
                //StartOfRound.Instance.OnClientDisconnect(3);
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }
    }
}
