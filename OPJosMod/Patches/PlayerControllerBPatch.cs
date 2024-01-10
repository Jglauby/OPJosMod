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

                    //"kill" server side
                    //killPlayerServer(__instance, (int)__instance.playerClientId, spawnBody, bodyVelocity, (int)causeOfDeath, deathAnimation);
                    System.Reflection.MethodInfo killPlayerServerRpc = typeof(PlayerControllerB).GetMethod("KillPlayerServerRpc", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (killPlayerServerRpc != null)
                    {
                        // Invoke the private method with the provided parameters
                        killPlayerServerRpc.Invoke(__instance, new object[] { (int)__instance.playerClientId, spawnBody, bodyVelocity, causeOfDeath, deathAnimation });
                    }
                    else
                    {
                        mls.LogMessage("killPlayerServerRpc method not found");
                    }
                }
            }
            
            throw new Exception("actually don't kill");
        }

        //snippet from KillPlayerServerRpc, goal is to despawn me, and drop a dead body
        private static void killPlayerServer(PlayerControllerB __instance, int playerId, bool spawnBody, Vector3 bodyVelocity, int causeOfDeath, int deathAnimation)
        {
            NetworkManager networkManager = __instance.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            // Use reflection to access the protected internal field __rpc_exec_stage
            FieldInfo rpcExecStageField = typeof(PlayerControllerB).GetField("__rpc_exec_stage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            __RpcExecStage rpcExecStage = (__RpcExecStage)rpcExecStageField.GetValue(__instance);

            if (rpcExecStage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {
                if (__instance.OwnerClientId != networkManager.LocalClientId)
                {
                    if (networkManager.LogLevel <= Unity.Netcode.LogLevel.Normal)
                    {
                        Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
                    }
                    return;
                }

                ServerRpcParams serverRpcParams = default(ServerRpcParams);
                FastBufferWriter bufferWriter = __beginSendServerRpc(1346025125u, serverRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValueBitPacked(bufferWriter, playerId);
                bufferWriter.WriteValueSafe(in spawnBody, default(FastBufferWriter.ForPrimitives));
                bufferWriter.WriteValueSafe(in bodyVelocity);
                BytePacker.WriteValueBitPacked(bufferWriter, causeOfDeath);
                BytePacker.WriteValueBitPacked(bufferWriter, deathAnimation);

                MethodInfo endSendServerRpcMethod = typeof(PlayerControllerB).GetMethod("__endSendServerRpc", BindingFlags.Instance | BindingFlags.NonPublic);
                endSendServerRpcMethod.Invoke(__instance, new object[] { bufferWriter, 1346025125u, serverRpcParams, RpcDelivery.Reliable });
                //__endSendServerRpc(ref bufferWriter, 1346025125u, serverRpcParams, RpcDelivery.Reliable);

                return;
            }

            if (rpcExecStage != __RpcExecStage.Server || (!networkManager.IsServer && !networkManager.IsHost))
            {
                return;
            }

            //__instance.playersManager.livingPlayers--;
            //if (__instance.playersManager.livingPlayers == 0)
            //{
            //    __instance.playersManager.allPlayersDead = true;
            //    __instance.playersManager.ShipLeaveAutomatically();
            //}

            if (!spawnBody)
            {
                PlayerControllerB component = __instance.playersManager.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
                for (int i = 0; i < component.ItemSlots.Length; i++)
                {
                    GrabbableObject grabbableObject = component.ItemSlots[i];
                    if (grabbableObject != null)
                    {
                        grabbableObject.gameObject.GetComponent<NetworkObject>().Despawn();
                    }
                }
            }
            else
            {
                GameObject obj = UnityEngine.Object.Instantiate(StartOfRound.Instance.ragdollGrabbableObjectPrefab, __instance.playersManager.propsContainer);
                obj.GetComponent<NetworkObject>().Spawn();
                obj.GetComponent<RagdollGrabbableObject>().bodyID.Value = (int)__instance.playerClientId;
            }

            return;
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
            }          
        }

        //slightly modified version of ReviveDeadPlayers from the StartOfRound object
        private static void ReviveDeadPlayer(PlayerControllerB __instance)
        {
            Vector3 respawnLocation = new Vector3 (0, 0, 0);

            var playerScripts = __instance.playersManager.allPlayerScripts;
            foreach (PlayerControllerB script in playerScripts)
            {

            }

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

                if (__instance.deadBody != null)
                {
                    respawnLocation = __instance.deadBody.transform.position;
                }
                else
                {
                    respawnLocation = __instance.transform.position;
                }
                __instance.TeleportPlayer(respawnLocation);

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

            RagdollGrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>();
            RagdollGrabbableObject myDeadRagdoll = null;
            foreach (RagdollGrabbableObject ragdoll in array)
            {
                if (ragdoll.ragdoll.playerScript.playerClientId == __instance.deadBody.playerScript.playerClientId)
                {
                    myDeadRagdoll = ragdoll;
                }
            }

            //if (myDeadRagdoll != null)
            //{
            //    if (myDeadRagdoll.isHeld)
            //    {
            //        if (__instance.IsServer)//was base.IsServer and this code is from StartOfRound
            //        {
            //            if (myDeadRagdoll.NetworkObject.IsSpawned)
            //            {
            //                myDeadRagdoll.NetworkObject.Despawn();
            //            }
            //            else
            //            {
            //                UnityEngine.Object.Destroy(myDeadRagdoll.gameObject);
            //            }
            //        }
            //    }
            //    else if (myDeadRagdoll.isHeld && myDeadRagdoll.playerHeldBy != null)
            //    {
            //        myDeadRagdoll.playerHeldBy.DropAllHeldItems();
            //    }
            //}

            DeadBodyInfo[] array2 = UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>();
            for (int k = 0; k < array2.Length; k++)
            {
                UnityEngine.Object.Destroy(array2[k].gameObject);
            }

            //StartOfRound startOfRound = new StartOfRound();
            //startOfRound.UpdatePlayerVoiceEffects();

            //ResetMiscValues();
            //revivePlayerServer(__instance);
        }

        //snagged from PlayerHasRevivedServerRpc from StartOfRound
        private static void revivePlayerServer(PlayerControllerB __instance)
        {
            
        }
    }
}
