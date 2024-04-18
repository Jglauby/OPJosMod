using BepInEx.Logging;
using GameNetcodeStuff;
using static Unity.Netcode.FastBufferWriter;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMod.ReviveCompany.CustomRpc
{
    public static class CompleteRecievedTasks
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void ModActivated(string modName)
        {
            if (mls.SourceName == modName)
            {
                mls.LogMessage("Mod Activated");
                GlobalVariables.ModActivated = true;
            }                
        }

        public static void RevivePlayer(string position)
        {
            Vector3 revivedPosition = GeneralUtil.StringToVector3(position);
            PlayerControllerB component = GeneralUtil.GetClosestDeadPlayer(revivedPosition);
            int playerIndex = (int)component.playerClientId;

            component.ResetPlayerBloodObjects(component.isPlayerDead);
            component.isClimbingLadder = false;
            component.ResetZAndXRotation();
            ((Collider)component.thisController).enabled = true;
            component.health = 100;
            component.disableLookInput = false;
            Debug.Log((object)"Reviving players B");
            if (component.isPlayerDead)
            {
                component.isPlayerDead = false;
                component.isPlayerControlled = true;
                component.isInElevator = true;
                component.isInHangarShipRoom = true;
                component.isInsideFactory = false;
                component.wasInElevatorLastFrame = false;
                StartOfRound.Instance.SetPlayerObjectExtrapolate(false);
                component.TeleportPlayer(revivedPosition, false, 0f, false, true); //adjust for reviving??
                component.setPositionOfDeadPlayer = false;
                component.DisablePlayerModel(StartOfRound.Instance.allPlayerObjects[playerIndex], true, true);
                ((Behaviour)component.helmetLight).enabled = false;
                Debug.Log((object)"Reviving players C");
                component.Crouch(false);
                component.criticallyInjured = false;
                if ((Object)(object)component.playerBodyAnimator != (Object)null)
                {
                    component.playerBodyAnimator.SetBool("Limp", false);
                }
                component.bleedingHeavily = false;
                component.activatingItem = false;
                component.twoHanded = false;
                component.inSpecialInteractAnimation = false;
                component.disableSyncInAnimation = false;
                component.inAnimationWithEnemy = null;
                component.holdingWalkieTalkie = false;
                component.speakingToWalkieTalkie = false;
                Debug.Log((object)"Reviving players D");
                component.isSinking = false;
                component.isUnderwater = false;
                component.sinkingValue = 0f;
                component.statusEffectAudio.Stop();
                component.DisableJetpackControlsLocally();
                component.health = 100;
                Debug.Log((object)"Reviving players E");
                component.mapRadarDotAnimator.SetBool("dead", false);
                HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                component.hasBegunSpectating = false;
                HUDManager.Instance.RemoveSpectateUI();
                HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                component.hinderedMultiplier = 1f;
                component.isMovementHindered = 0;
                component.sourcesCausingSinking = 0;
                Debug.Log((object)"Reviving players E2");
                component.reverbPreset = StartOfRound.Instance.shipReverb;
            }
            Debug.Log((object)"Reviving players F");
            SoundManager.Instance.earsRingingTimer = 0f;
            component.voiceMuffledByEnemy = false;
            SoundManager.Instance.playerVoicePitchTargets[playerIndex] = 1f;
            SoundManager.Instance.SetPlayerPitch(1f, playerIndex);
            if ((Object)(object)component.currentVoiceChatIngameSettings == (Object)null)
            {
                StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
            }
            if ((Object)(object)component.currentVoiceChatIngameSettings != (Object)null)
            {
                if ((Object)(object)component.currentVoiceChatIngameSettings.voiceAudio == (Object)null)
                {
                    component.currentVoiceChatIngameSettings.InitializeComponents();
                }
                if ((Object)(object)component.currentVoiceChatIngameSettings.voiceAudio == (Object)null)
                {
                    return;
                }
                ((Component)component.currentVoiceChatIngameSettings.voiceAudio).GetComponent<OccludeAudio>().overridingLowPass = false;
            }
            Debug.Log((object)"Reviving players G");
            PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
            localPlayerController.bleedingHeavily = false;
            localPlayerController.criticallyInjured = false;
            localPlayerController.playerBodyAnimator.SetBool("Limp", false);
            localPlayerController.health = 100;
            HUDManager.Instance.UpdateHealthUI(100, false);
            localPlayerController.spectatedPlayerScript = null;
            ((Behaviour)HUDManager.Instance.audioListenerLowPass).enabled = false;
            Debug.Log((object)"Reviving players H");
            StartOfRound.Instance.SetSpectateCameraToGameOverMode(false, localPlayerController);
            RagdollGrabbableObject[] array = Object.FindObjectsOfType<RagdollGrabbableObject>();
            for (int i = 0; i < array.Length; i++)
            {
                if (!((GrabbableObject)array[i]).isHeld)
                {
                    if (StartOfRound.Instance.IsHost)    //if (((NetworkBehaviour)this).IsServer)
                    {
                        if (((NetworkBehaviour)array[i]).NetworkObject.IsSpawned)
                        {
                            ((NetworkBehaviour)array[i]).NetworkObject.Despawn(true);
                        }
                        else
                        {
                            Object.Destroy((Object)(object)((Component)array[i]).gameObject);
                        }
                    }
                }
                else if (((GrabbableObject)array[i]).isHeld && (Object)(object)((GrabbableObject)array[i]).playerHeldBy != (Object)null)
                {
                    ((GrabbableObject)array[i]).playerHeldBy.DropAllHeldItems(true, false);
                }
            }
            DeadBodyInfo[] array2 = Object.FindObjectsOfType<DeadBodyInfo>();
            for (int j = 0; j < array2.Length; j++)
            {
                Object.Destroy((Object)(object)((Component)array2[j]).gameObject);
            }
            StartOfRound instance = StartOfRound.Instance;
            instance.livingPlayers++;
            StartOfRound.Instance.allPlayersDead = false;
            StartOfRound.Instance.UpdatePlayerVoiceEffects();
        }
    }
}
