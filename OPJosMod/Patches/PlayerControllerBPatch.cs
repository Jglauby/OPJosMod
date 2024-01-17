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

        private static IEnumerator killSpawnedEnemy(PlayerControllerB __instance, float time, MaskedPlayerEnemy spawnedEnemy)
        {
            mls.LogMessage("Before delay - Time: " + Time.time);
            yield return new WaitForSeconds(time);
            mls.LogMessage("After delay - Time: " + Time.time);

            try
            {
                //fix variables for function call
                spawnedEnemy.ventAnimationFinished = true;
                __instance.isPlayerControlled = true;
                __instance.isPlayerDead = false;

                mls.LogMessage($" if (!(stunNormalizedTimer:{spawnedEnemy.stunNormalizedTimer} >= 0f) && !isEnemyDead:{!spawnedEnemy.isEnemyDead} && !(Time.realtimeSinceStartup:{Time.realtimeSinceStartup} - timeAtLastUsingEntrance:unsure/privatefield < 1.75f))");
                mls.LogMessage("MeetsStandardPlayerCollisionConditions:");
                mls.LogMessage($"inKillAnimation = (inKillAnimation:privateField || startingKillAnimationLocalClient:{spawnedEnemy.startingKillAnimationLocalClient} || !enemyEnabled:privateField)");
                mls.LogMessage($"isEnemyDead:{spawnedEnemy.isEnemyDead}");
                mls.LogMessage($"!ventAnimationFinished:{!spawnedEnemy.ventAnimationFinished}");
                mls.LogMessage($"inKillAnimation:privateField/result of earlier debug");
                mls.LogMessage($"stunNormalizedTimer:{spawnedEnemy.stunNormalizedTimer} >= 0f");
                mls.LogMessage("PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();");
                mls.LogMessage($"component:{spawnedEnemy.gameObject} == null || component:{__instance.gameObject.GetComponent<PlayerControllerB>()} != GameNetworkManager.Instance.localPlayerController:{GameNetworkManager.Instance.localPlayerController}");
                mls.LogMessage($"if (playerScript.isPlayerControlled:{__instance.isPlayerControlled} && !playerScript.isPlayerDead:{!__instance.isPlayerDead} && playerScript.inAnimationWithEnemy:{__instance.inAnimationWithEnemy} == null && (overrideInsideFactoryCheck:{false} || playerScript.isInsideFactory:{__instance.isInsideFactory} != isOutside:{spawnedEnemy.isOutside}) && playerScript.sinkingValue:{__instance.sinkingValue} < 0.73f)");

                __instance.playerCollider.transform.position = spawnedEnemy.transform.position;//cause player is now below ground
                spawnedEnemy.OnCollideWithPlayer(__instance.playerCollider);

                //fix back? maybe
                spawnedEnemy.ventAnimationFinished = false;
                __instance.isPlayerControlled = false;
                __instance.isPlayerDead = true;


                //NetworkManager.Destroy(spawnedEnemy.gameObject);

                //spawnedEnemy.enemyHP = 0;
                //spawnedEnemy.HitEnemy(1, __instance);
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
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
                //__instance.SpawnDeadBody((int)__instance.playerClientId, __instance.velocityLastFrame, (int)__instance.causeOfDeath, __instance, deathAnimation);

                //instead of spawning dead body, spawn and instant kill mimic body of my name via server sends. 
                spawnFakeDeadBody(__instance, deathAnimation, deathLocation);
            }

            StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);
            __instance.isInGameOverAnimation = 1.5f;
            __instance.cursorTip.text = "";
            __instance.cursorIcon.enabled = false;
            //__instance.DropAllHeldItems(true);
            __instance.DisableJetpackControlsLocally();
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void patchKillPlayer(PlayerControllerB __instance, ref int deathAnimation, ref bool spawnBody, ref Vector3 bodyVelocity, ref CauseOfDeath causeOfDeath)
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

        private static void spawnFakeDeadBody(PlayerControllerB __instance, int deathAnimation, Vector3 deathLocation)
        {
            mls.LogMessage("loading fake dead body");//maybe there is a test body you can spawn? like dumby player?
            try
            {
                Vector3 spawnPosition = deathLocation;
                float yRot = __instance.transform.rotation.eulerAngles.y;
                EnemyType enemyType = StartOfRound.Instance.levels[5].Enemies[10].enemyType; //5 level rend, 10 maskedPlayerEnemy    , this part fails when not host, its null?

                //debuggin info sent to spawn function
                //mls.LogMessage($"spawnPosition: {spawnPosition}");
                //mls.LogMessage($"yRot: {yRot}");
                //mls.LogMessage($"enemyType name: {enemyType.name}");
                //mls.LogMessage($"enemyType prefab: {enemyType.enemyPrefab}");
                //int enemyCount = StartOfRound.Instance.currentLevel.Enemies.Count;
                //mls.LogMessage("enemy array size: " + enemyCount);
                //for (int i = 0; i < enemyCount; i++)
                //{
                //    mls.LogMessage($"enemy {i}: {StartOfRound.Instance.currentLevel.Enemies[i].enemyType.name}");
                //}
                var spawnedBody = RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, yRot, -1, enemyType);

                mls.LogMessage("kill spawned enemy");
                List<EnemyAI> array = RoundManager.Instance.SpawnedEnemies;
                mls.LogMessage($"enemies found in round {array.Count()}");
                
                if (array.Count > 0) 
                {
                    EnemyAI closestEnemy = array[0];
                    float minDistance = Vector3.Distance(array[0].transform.position, spawnPosition);
                
                    for (int i = 1; i < array.Count; i++)
                    {
                        float distance = Vector3.Distance(array[i].transform.position, spawnPosition);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestEnemy = array[i];
                        }
                    }
                    mls.LogMessage($"kill closest enemy {closestEnemy}");
                    closestEnemy.Start();                    

                    MaskedPlayerEnemy spawnedMaskedPlayer = closestEnemy as MaskedPlayerEnemy;
                    spawnedMaskedPlayer.creatureAnimator = closestEnemy.gameObject.GetComponentInChildren<Animator>();
                    

                    __instance.StartCoroutine(killSpawnedEnemy(__instance, 0.25f, spawnedMaskedPlayer));
                }
                else
                {
                    mls.LogError("couldn't find the spawned enemy");
                }

                //debuggin level enemies, shows all enemies that can spawn on each level
                //var allLevels = StartOfRound.Instance.levels;
                //mls.LogMessage("all levels");
                //for (int i = 0; i < allLevels.Count(); i++)
                //{
                //    mls.LogMessage($"level {i}: {allLevels[i].name}");
                //    foreach(var enemy in allLevels[i].Enemies)
                //    {
                //        mls.LogMessage($"enemy: {enemy.enemyType.name}");
                //    }
                //}
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }

        [HarmonyPatch("DamagePlayer")]
        [HarmonyPostfix]
        static void patchDamagePlayer(PlayerControllerB __instance)
        {
            //__instance.health = 100;
            //HUDManager.Instance.UpdateHealthUI(__instance.health, false);
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
                if (__instance.deadBody != null)
                {
                    //respawnLocation = __instance.deadBody.transform.position;
                }
                else
                {
                    //respawnLocation = __instance.transform.position;
                }

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
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }
    }
}
