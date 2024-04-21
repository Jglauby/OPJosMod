using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OPJosMod.ReviveCompany
{
    public static class GeneralUtil
    {
        public static Vector3 StringToVector3(string str)
        {
            string[] components = str.Trim('(', ')').Split(',');

            float x = float.Parse(components[0]);
            float y = float.Parse(components[1]);
            float z = float.Parse(components[2]);

            return new Vector3(x, y, z);
        }

        public static PlayerControllerB GetClosestDeadPlayer(Vector3 position)
        {
            PlayerControllerB closestPlayer = null;
            float closestDistance = float.MaxValue;

            PlayerControllerB[] allPlayers = GameObject.FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB player in allPlayers)
            {
                if (player.isPlayerDead)
                {
                    float distance = Vector3.Distance(player.transform.position, position);
                    if (distance < closestDistance)
                    {
                        closestPlayer = player;
                        closestDistance = distance;
                    }
                }
            }

            return closestPlayer;
        }

        public static PlayerControllerB GetClosestAlivePlayer(Vector3 position)
        {
            PlayerControllerB closestPlayer = null;
            float closestDistance = float.MaxValue;

            PlayerControllerB[] allPlayers = GameObject.FindObjectsOfType<PlayerControllerB>();
            foreach (PlayerControllerB player in allPlayers)
            {
                if (!player.isPlayerDead)
                {
                    float distance = Vector3.Distance(player.transform.position, position);
                    if (distance < closestDistance)
                    {
                        closestPlayer = player;
                        closestDistance = distance;
                    }
                }
            }

            return closestPlayer;
        }

        public static string simplifyObjectNames(string ogName)
        {
            var objectName = ogName;
            int index = objectName.IndexOf("(");
            if (index != -1)
                objectName = objectName.Substring(0, index).Trim();

            return objectName;
        }

        public static void RevivePlayer(Vector3 revivedPosition)
        {
            PlayerControllerB closestAlivePlayer = GeneralUtil.GetClosestAlivePlayer(revivedPosition);
            PlayerControllerB player = GeneralUtil.GetClosestDeadPlayer(revivedPosition);
            int playerIndex = (int)player.playerClientId;

            player.ResetPlayerBloodObjects(player.isPlayerDead);
            player.isClimbingLadder = false;
            player.ResetZAndXRotation();
            ((Collider)player.thisController).enabled = true;
            player.health = 100;
            player.disableLookInput = false;
            if (player.isPlayerDead)
            {
                player.isPlayerDead = false;
                player.isPlayerControlled = true;
                player.isInElevator = true;
                player.isInHangarShipRoom = true;
                player.isInsideFactory = closestAlivePlayer.isInsideFactory;
                player.wasInElevatorLastFrame = false;
                StartOfRound.Instance.SetPlayerObjectExtrapolate(false);
                player.TeleportPlayer(revivedPosition, false, 0f, false, true); //adjust for reviving??
                player.setPositionOfDeadPlayer = false;
                player.DisablePlayerModel(StartOfRound.Instance.allPlayerObjects[playerIndex], true, true);
                ((Behaviour)player.helmetLight).enabled = false;
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
                player.isSinking = false;
                player.isUnderwater = false;
                player.sinkingValue = 0f;
                player.statusEffectAudio.Stop();
                player.DisableJetpackControlsLocally();
                player.health = 100;
                player.mapRadarDotAnimator.SetBool("dead", false);
                HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
                player.hasBegunSpectating = false;
                HUDManager.Instance.RemoveSpectateUI();
                HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
                player.hinderedMultiplier = 1f;
                player.isMovementHindered = 0;
                player.sourcesCausingSinking = 0;
                player.reverbPreset = StartOfRound.Instance.shipReverb;
            }
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

            PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
            if (localPlayerController.playerClientId == player.playerClientId)
            {
                localPlayerController.bleedingHeavily = false;
                localPlayerController.criticallyInjured = false;
                localPlayerController.playerBodyAnimator.SetBool("Limp", false);
                localPlayerController.health = 100;
                HUDManager.Instance.UpdateHealthUI(100, true);
                localPlayerController.spectatedPlayerScript = null;
                ((Behaviour)HUDManager.Instance.audioListenerLowPass).enabled = false;
                StartOfRound.Instance.SetSpectateCameraToGameOverMode(false, localPlayerController);
            }

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
