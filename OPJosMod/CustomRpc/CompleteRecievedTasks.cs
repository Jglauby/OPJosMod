﻿using BepInEx.Logging;
using GameNetcodeStuff;
using static Unity.Netcode.FastBufferWriter;
using Unity.Netcode;
using UnityEngine;
using OPJosMod.GhostMode.Patches;

namespace OPJosMod.GhostMode.CustomRpc
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

        public static void TurnOffGhostMode(string clientID)
        {
            if (int.TryParse(clientID, out var clientIDInt))
            {
                if ((int)StartOfRound.Instance.localPlayerController.playerClientId == clientIDInt && PlayerControllerBPatch.isGhostMode)
                {
                    //teleport to dead body
                    mls.LogMessage("Revived on Other Client, turning off ghost mode.");
                    var player = StartOfRound.Instance.localPlayerController;
                    player.TeleportPlayer(player.deadBody.transform.position);

                    //delete closest dead body
                    RagdollGrabbableObject closestBody = null;
                    float closestDistance = float.MaxValue;

                    RagdollGrabbableObject[] allDeadBodies = GameObject.FindObjectsOfType<RagdollGrabbableObject>();
                    foreach (RagdollGrabbableObject body in allDeadBodies)
                    {
                        float distance = Vector3.Distance(body.transform.position, player.transform.position);
                        if (distance < closestDistance)
                        {
                            closestBody = body;
                            closestDistance = distance;
                        }
                    }
                    if (closestBody != null)
                    {
                        if (!((GrabbableObject)closestBody).isHeld)
                        {
                            if (StartOfRound.Instance.IsServer)    //if (((NetworkBehaviour)this).IsServer)
                            {
                                if (((NetworkBehaviour)closestBody).NetworkObject.IsSpawned)
                                {
                                    ((NetworkBehaviour)closestBody).NetworkObject.Despawn(true);
                                }
                                else
                                {
                                    Object.Destroy((Object)(object)((Component)closestBody).gameObject);
                                }
                            }
                        }
                        else if (((GrabbableObject)closestBody).isHeld && (Object)(object)((GrabbableObject)closestBody).playerHeldBy != (Object)null)
                        {
                            ((GrabbableObject)closestBody).playerHeldBy.DropAllHeldItems(true, false);
                        }

                        if (closestBody.ragdoll != null)
                            Object.Destroy((Object)(object)((Component)closestBody.ragdoll).gameObject);
                    }

                    PlayerControllerBPatch.resetGhostModeVars(player);
                }
            }          
        }

        public static void OtherPlayerGhostToggle(string message)
        {
            //message should be in this format "playerClientID,0" -> where 0 is false and a 1 would be true
            //got to make sure if you get told to turn off ghost toggle, that it also toggles this to not show other ghosts
        }
    }
}
