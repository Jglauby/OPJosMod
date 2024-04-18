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
            PlayerControllerB player = GeneralUtil.GetClosestDeadPlayer(revivedPosition);
            int playerIndex = (int)player.playerClientId;

            player.ResetPlayerBloodObjects(player.isPlayerDead);
            player.isClimbingLadder = false;
            player.ResetZAndXRotation();
            ((Collider)player.thisController).enabled = true;
            player.health = 100;
            player.disableLookInput = false;
            Debug.Log((object)"Reviving players B");
            if (player.isPlayerDead)
            {
                player.isPlayerDead = false;
                player.isPlayerControlled = true;
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = false;
                player.wasInElevatorLastFrame = false;
                StartOfRound.Instance.SetPlayerObjectExtrapolate(false);
                player.TeleportPlayer(revivedPosition, false, 0f, false, true); //adjust for reviving??
                player.setPositionOfDeadPlayer = false;
                player.DisablePlayerModel(StartOfRound.Instance.allPlayerObjects[playerIndex], true, true);
                ((Behaviour)player.helmetLight).enabled = false;
                Debug.Log((object)"Reviving players C");
                player.Crouch(false);
                player.criticallyInjured = false;
                if ((Object)(object)player.playerBodyAnimator != (Object)null)
                {
                    player.playerBodyAnimator.SetBool("Limp", false);
                }
                player.bleedingHeavily = false;
                player.activatingItem = false;
                player.twoHanded = false;
                player.inSpecialInteractAnimation = false;
                player.disableSyncInAnimation = false;
                player.inAnimationWithEnemy = null;
                player.holdingWalkieTalkie = false;
                player.speakingToWalkieTalkie = false;
                Debug.Log((object)"Reviving players D");
                player.isSinking = false;
                player.isUnderwater = false;
                player.sinkingValue = 0f;
                player.statusEffectAudio.Stop();
                player.DisableJetpackControlsLocally();
                player.health = 100;
                Debug.Log((object)"Reviving players E");
                player.mapRadarDotAnimator.SetBool("dead", false);
                HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                player.hasBegunSpectating = false;
                HUDManager.Instance.RemoveSpectateUI();
                HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                player.hinderedMultiplier = 1f;
                player.isMovementHindered = 0;
                player.sourcesCausingSinking = 0;
                Debug.Log((object)"Reviving players E2");
                player.reverbPreset = StartOfRound.Instance.shipReverb;
            }
            Debug.Log((object)"Reviving players F");
            SoundManager.Instance.earsRingingTimer = 0f;
            player.voiceMuffledByEnemy = false;
            SoundManager.Instance.playerVoicePitchTargets[playerIndex] = 1f;
            SoundManager.Instance.SetPlayerPitch(1f, playerIndex);
            if ((Object)(object)player.currentVoiceChatIngameSettings == (Object)null)
            {
                StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
            }
            if ((Object)(object)player.currentVoiceChatIngameSettings != (Object)null)
            {
                if ((Object)(object)player.currentVoiceChatIngameSettings.voiceAudio == (Object)null)
                {
                    player.currentVoiceChatIngameSettings.InitializeComponents();
                }
                if ((Object)(object)player.currentVoiceChatIngameSettings.voiceAudio == (Object)null)
                {
                    return;
                }
                ((Component)player.currentVoiceChatIngameSettings.voiceAudio).GetComponent<OccludeAudio>().overridingLowPass = false;
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
