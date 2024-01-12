using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.GodMode.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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

        private static FastBufferWriter __beginSendServerRpc(uint rpcMethodId, ServerRpcParams serverRpcParams, RpcDelivery rpcDelivery)
        {
            return new FastBufferWriter(1024, Allocator.Temp, 65536);
        }

        private static FastBufferWriter __beginSendClientRpc(uint rpcMethodId, ClientRpcParams clientRpcParams, RpcDelivery rpcDelivery)
        {
            return new FastBufferWriter(1024, Allocator.Temp, 65536);
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void patchKillPlayer(PlayerControllerB __instance, ref int deathAnimation, ref bool spawnBody, ref Vector3 bodyVelocity, ref CauseOfDeath causeOfDeath)
        {
            mls.LogMessage("died kinda");
            MethodInfo methodInfo = AccessTools.Method(typeof(PlayerControllerB), "KillPlayerServerRpc");
            FieldInfo wasUnderWaterLastFrameField = typeof(PlayerControllerB).GetField("wasUnderwaterLastFrame", BindingFlags.NonPublic | BindingFlags.Instance);

            if (wasUnderWaterLastFrameField != null && methodInfo != null)
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


                    //spawn dead body server
                    //spawnDeadBodyServer(__instance);

                    System.Reflection.MethodInfo killPlayerServerRpc = typeof(PlayerControllerB).GetMethod("KillPlayerServerRpc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (killPlayerServerRpc != null)
                    {
                        // Invoke the private method with the provided parameters
                        //killPlayerServerRpc.Invoke(__instance, new object[] { (int)__instance.playerClientId, spawnBody, bodyVelocity, causeOfDeath, deathAnimation });
                    }
                    else
                    {
                        mls.LogMessage("killPlayerServerRpc method not found");
                    }
                }
            }

            throw new Exception("actually don't kill");
        }

        private static void spawnDeadBodyServer(PlayerControllerB __instance)
        {
            mls.LogMessage("entered spawn dead body server function");

            try
            {
                mls.LogMessage("trying to add a new dumby character to server");
                ulong clientId = __instance.playerClientId;
                int connectedPlayers = __instance.playersManager.allPlayerScripts.Length; // Assuming at least one player is already connected
                ulong[] connectedPlayerIdsOrdered = __instance.playersManager.allPlayerScripts.OrderBy(x => x.playerClientId).Select(x => x.playerClientId).ToArray();
                int assignedPlayerObjectId = Array.IndexOf(connectedPlayerIdsOrdered, clientId);
                int serverMoneyAmount = 0; // Provide an appropriate amount
                int levelID = __instance.playersManager.currentLevelID; // Provide an appropriate level ID
                int profitQuota = TimeOfDay.Instance.profitQuota; // Provide an appropriate profit quota
                int timeUntilDeadline = (int)TimeOfDay.Instance.timeUntilDeadline; // Provide an appropriate time until the deadline
                int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled; // Provide an appropriate value
                int randomSeed = __instance.playersManager.randomMapSeed; // Provide an appropriate random seed
                bool isChallenge = __instance.playersManager.isChallengeFile; // Revive scenario might not be a challenge

                // Call the OnPlayerConnectedClientRpc method with simulated parameters
                MethodInfo methodInfo = typeof(StartOfRound).GetMethod("OnPlayerConnectedClientRpc", BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo.Invoke(__instance.playersManager, new object[] { clientId, connectedPlayers, connectedPlayerIdsOrdered, assignedPlayerObjectId,
                    serverMoneyAmount, levelID, profitQuota, timeUntilDeadline, quotaFulfilled, randomSeed, isChallenge });

                __instance.playersManager.PlayerHasRevivedServerRpc();
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }

        [HarmonyPatch("KillPlayerServerRpc")]
        [HarmonyPrefix]
        static void patchKillPlayerServerRpc(PlayerControllerB __instance, ref int playerId, ref bool spawnBody, ref Vector3 bodyVelocity, ref int causeOfDeath, ref int deathAnimation)
        {
            mls.LogMessage("KillPlayerServerRpc hit");

            //System.Reflection.MethodInfo killPlayerClientRpc = typeof(PlayerControllerB).GetMethod("KillPlayerClientRpc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //if (killPlayerClientRpc != null)
            //{
            //    // Invoke the private method with the provided parameters
            //    killPlayerClientRpc.Invoke(__instance, new object[] { playerId, spawnBody, bodyVelocity, causeOfDeath, deathAnimation });
            //}
            //else
            //{
            //    mls.LogMessage("KillPlayerClientRpc method not found");
            //}
            //
            //throw new Exception("don't call the actually kill player rpc");
        }

        [HarmonyPatch("KillPlayerClientRpc")]
        [HarmonyPrefix]
        static void patchKillPlayerClientRpc(PlayerControllerB __instance)
        {
            mls.LogMessage("KillPlayerClientRpc hit");
          
            //throw new Exception("don't call the actually kill player rpc");
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
                //respawnPlayerServer(__instance);
            }          
        }

        //slightly modified version of ReviveDeadPlayers from the StartOfRound object
        private static void ReviveDeadPlayer(PlayerControllerB __instance)
        {
            Vector3 respawnLocation = new Vector3 (0, 0, 0);
            if (__instance.deadBody != null)
            {
                respawnLocation = __instance.deadBody.transform.position;
            }
            else
            {
                respawnLocation = __instance.transform.position;
            }

            var allPlayerScripts = __instance.playersManager.allPlayerScripts;
            var playerIndex = (int)__instance.playerClientId;
            //
            __instance.playersManager.allPlayersDead = false;

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

            mls.LogMessage("Reviving players B");

            if (allPlayerScripts[playerIndex].isPlayerDead)
            {
                // Revival process for the specified player
                allPlayerScripts[playerIndex].isPlayerDead = false;
                allPlayerScripts[playerIndex].isPlayerControlled = true;
                allPlayerScripts[playerIndex].isInElevator = true;
                allPlayerScripts[playerIndex].isInHangarShipRoom = true;
                allPlayerScripts[playerIndex].isInsideFactory = false;
                allPlayerScripts[playerIndex].wasInElevatorLastFrame = false;
                __instance.playersManager.SetPlayerObjectExtrapolate(enable: false);
                allPlayerScripts[playerIndex].TeleportPlayer(respawnLocation);
                allPlayerScripts[playerIndex].setPositionOfDeadPlayer = false;
                allPlayerScripts[playerIndex].DisablePlayerModel(__instance.playersManager.allPlayerObjects[playerIndex], enable: true, disableLocalArms: true);
                allPlayerScripts[playerIndex].helmetLight.enabled = false;

                mls.LogMessage("Reviving players C");

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

                mls.LogMessage("Reviving players D");

                allPlayerScripts[playerIndex].isSinking = false;
                allPlayerScripts[playerIndex].isUnderwater = false;
                allPlayerScripts[playerIndex].sinkingValue = 0f;
                allPlayerScripts[playerIndex].statusEffectAudio.Stop();
                allPlayerScripts[playerIndex].DisableJetpackControlsLocally();
                allPlayerScripts[playerIndex].health = 100;

                mls.LogMessage("Reviving players E");

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

                    mls.LogMessage("Reviving players E2");

                    allPlayerScripts[playerIndex].reverbPreset = __instance.playersManager.shipReverb;
                }
            }

            mls.LogMessage("Reviving players F");

            // Additional revival steps for the specified player
            SoundManager.Instance.earsRingingTimer = 0f;
            allPlayerScripts[playerIndex].voiceMuffledByEnemy = false;
            SoundManager.Instance.playerVoicePitchTargets[playerIndex] = 1f;
            SoundManager.Instance.SetPlayerPitch(1f, playerIndex);

            if (allPlayerScripts[playerIndex].currentVoiceChatIngameSettings == null)
            {
                __instance.playersManager.RefreshPlayerVoicePlaybackObjects();
            }

            if (allPlayerScripts[playerIndex].currentVoiceChatIngameSettings != null &&
                allPlayerScripts[playerIndex].currentVoiceChatIngameSettings.voiceAudio != null)
            {
                allPlayerScripts[playerIndex].currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
            }

            mls.LogMessage("Reviving players G");

            PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
            playerControllerB.bleedingHeavily = false;
            playerControllerB.criticallyInjured = false;
            playerControllerB.playerBodyAnimator.SetBool("Limp", value: false);
            playerControllerB.health = 100;
            HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
            playerControllerB.spectatedPlayerScript = null;
            HUDManager.Instance.audioListenerLowPass.enabled = false;
            mls.LogMessage("Reviving players H");
            __instance.playersManager.SetSpectateCameraToGameOverMode(enableGameOver: false, playerControllerB);
            RagdollGrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>();
            for (int j = 0; j < array.Length; j++)
            {
                if (!array[j].isHeld)
                {
                    if (__instance.playersManager.IsServer)
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
            __instance.playersManager.livingPlayers = __instance.playersManager.connectedPlayersAmount + 1;
            __instance.playersManager.allPlayersDead = false;
            __instance.playersManager.UpdatePlayerVoiceEffects();

            //__instance.playersManager.shipAnimator.ResetTrigger("ShipLeave");

            respawnPlayerServer(__instance);
        }

        private static void respawnPlayerServer(PlayerControllerB __instance)
        {
            mls.LogMessage("hit respawn player server");
            try
            {
                // Simulate relevant information for a new player
                //ulong clientId = __instance.playerClientId;
                //int connectedPlayers = __instance.playersManager.allPlayerScripts.Length; // Assuming at least one player is already connected
                //ulong[] connectedPlayerIdsOrdered = __instance.playersManager.allPlayerScripts.OrderBy(x => x.playerClientId).Select(x => x.playerClientId).ToArray();
                //int assignedPlayerObjectId = Array.IndexOf(connectedPlayerIdsOrdered, clientId);
                //int serverMoneyAmount = 0; // Provide an appropriate amount
                //int levelID = __instance.playersManager.currentLevelID; // Provide an appropriate level ID
                //int profitQuota = TimeOfDay.Instance.profitQuota; // Provide an appropriate profit quota
                //int timeUntilDeadline = (int)TimeOfDay.Instance.timeUntilDeadline; // Provide an appropriate time until the deadline
                //int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled; // Provide an appropriate value
                //int randomSeed = __instance.playersManager.randomMapSeed; // Provide an appropriate random seed
                //bool isChallenge = __instance.playersManager.isChallengeFile; // Revive scenario might not be a challenge
                //
                //// Call the OnPlayerConnectedClientRpc method with simulated parameters
                //MethodInfo methodInfo = typeof(StartOfRound).GetMethod("OnPlayerConnectedClientRpc", BindingFlags.NonPublic | BindingFlags.Instance);
                //methodInfo.Invoke(__instance.playersManager, new object[] { clientId, connectedPlayers, connectedPlayerIdsOrdered, assignedPlayerObjectId, 
                //    serverMoneyAmount, levelID, profitQuota, timeUntilDeadline, quotaFulfilled, randomSeed, isChallenge });



                //call UpdatePlayerPositionRpc
               //mls.LogMessage("trying to call UpdatePlayerPositionRpc");
               //mls.LogMessage(__instance);
               //Vector3 newPos = __instance.transform.position;
               //bool inElevator = __instance.isInElevator;
               //bool inShipRoom = __instance.isInHangarShipRoom;
               //bool exhausted = __instance.isExhausted;
               //bool isPlayerGrounded = __instance.isGroundedOnServer;
               //
               //MethodInfo methodInfo = typeof(PlayerControllerB).GetMethod("UpdatePlayerPositionServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
               //methodInfo.Invoke(__instance, new object[] { newPos, inElevator, inShipRoom, exhausted, isPlayerGrounded });

                //call player has revived
                mls.LogMessage("calling PlayerHasRevivedServerRpc");
                //__instance.playersManager.PlayerHasRevivedServerRpc();










                mls.LogMessage("undo died values kinda");
                FieldInfo wasUnderWaterLastFrameField = typeof(PlayerControllerB).GetField("wasUnderwaterLastFrame", BindingFlags.NonPublic | BindingFlags.Instance);

                if (wasUnderWaterLastFrameField != null)
                {
                    bool wasUnderWaterLastFrameValue = (bool)wasUnderWaterLastFrameField.GetValue(__instance);

                    //killplayer function
                    if (__instance.IsOwner && __instance.isPlayerDead)
                    {
                        __instance.isPlayerDead = false;
                        __instance.isPlayerControlled = true;
                        __instance.thisPlayerModelArms.enabled = true;
                        __instance.localVisor.position = __instance.playersManager.notSpawnedPosition.position;
                        __instance.DisablePlayerModel(__instance.gameObject, true);
                        //__instance.isInsideFactory = true;
                        //__instance.IsInspectingItem = false;
                        //__instance.inTerminalMenu = false;
                       // __instance.twoHanded = false;
                        //__instance.carryWeight = 1f;
                        //__instance.fallValue = 0f;
                        //__instance.fallValueUncapped = 0f;
                        //__instance.takingFallDamage = false;
                        //__instance.isSinking = false;
                        //__instance.isUnderwater = false;
                        StartOfRound.Instance.drowningTimer = 1f;
                        HUDManager.Instance.setUnderwaterFilter = false;
                        wasUnderWaterLastFrameField.SetValue(__instance, false);
                        __instance.sourcesCausingSinking = 0;
                        __instance.sinkingValue = 0f;
                        __instance.hinderedMultiplier = 1f;
                        __instance.isMovementHindered = 0;
                        __instance.inAnimationWithEnemy = null;
                        //UnityEngine.Object.FindObjectOfType<Terminal>().terminalInUse = false;
                        ChangeAudioListenerToObject(__instance, __instance.playersManager.spectateCamera.gameObject);
                        SoundManager.Instance.SetDiageticMixerSnapshot();
                        HUDManager.Instance.SetNearDepthOfFieldEnabled(enabled: true);
                        //HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", value: false);
                        //HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
                        HUDManager.Instance.HideHUD(hide: false);
                        StopHoldInteractionOnTrigger(__instance);

                        StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.activeCamera);
                        //__instance.isInGameOverAnimation = 1.5f;
                        //__instance.cursorTip.text = "";
                        __instance.cursorIcon.enabled = true;
                        //__instance.DropAllHeldItems(true);
                        //__instance.DisableJetpackControlsLocally();
                    }
                }
            }        
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }
    }
}
