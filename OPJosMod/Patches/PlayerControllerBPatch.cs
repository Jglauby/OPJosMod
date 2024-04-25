using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.ReviveCompany.CustomRpc;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OPJosMod.ReviveCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static int interactableObjectsMask = 832;
        private static float StartedReviveAt = Time.time;
        private static bool StartedRevive = false;

        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        private static bool interact_performedPatch(PlayerControllerB __instance)
        {
            if (!GlobalVariables.ModActivated)
                return true;

            if (__instance.IsOwner && !__instance.isPlayerDead && (!__instance.IsServer || __instance.isHostPlayerObject))
            {
                if (!canUse(__instance))
                    return false;
            }

            return true;
        }

        [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
        [HarmonyPostfix]
        private static void setHoverTipAndCurrentInteractTriggerPatch(PlayerControllerB __instance)
        {
            if (!GlobalVariables.ModActivated)
                return;

            if (!canUse(__instance) && __instance.cursorTip.text != "")
            {
                if (((ButtonControl)Keyboard.current[Key.E]).isPressed)//E is pressed
                {
                    if (StartedRevive == false)
                    {
                        StartedRevive = true;
                        StartedReviveAt = Time.time;
                    }
                    else if (Time.time - StartedReviveAt > ConfigVariables.reviveTime)
                    {
                        StartedRevive = false;
                        var revivingBody = GeneralUtil.GetClosestDeadBody(__instance.transform.position);
                        var revivingBodyPos = GeneralUtil.GetClosestDeadBodyPosition(__instance.transform.position);

                        //return if it is your body you are reviving
                        if (revivingBody != null && revivingBody.ragdoll != null && revivingBody.ragdoll.playerScript != null &&
                            GameNetworkManager.Instance.localPlayerController.playerClientId == revivingBody.ragdoll.playerScript.playerClientId)
                        {
                            mls.LogError("didn't revive as the dead body that was attempted to revive was the local player");
                            return;
                        }

                        //send revive message!
                        RpcMessage rpcMessage = new RpcMessage(MessageTasks.RevivePlayer, revivingBodyPos.ToString(), (int)__instance.playerClientId, MessageCodes.Request);
                        RpcMessageHandler.SendRpcMessage(rpcMessage);

                        GeneralUtil.RevivePlayer(revivingBodyPos);

                        //turn off ghost mode for the player
                        PlayerControllerB revivingPlayer = GeneralUtil.GetClosestAlivePlayer(revivingBodyPos);
                        RpcMessage rpcMessage2 = new RpcMessage(MessageTasks.TurnOffGhostMode, revivingPlayer.playerClientId.ToString(), (int)__instance.playerClientId, MessageCodes.Request);
                        RpcMessageHandler.SendRpcMessage(rpcMessage2);
                    }

                    __instance.cursorTip.text = $"Reviving! {(int)Mathf.Round(Time.time - StartedReviveAt)}/{ConfigVariables.reviveTime}s";
                }
                else
                {
                    __instance.cursorTip.text = "Hold E to revive!";
                    StartedRevive = false;
                }
            }
            else
            {
                StartedRevive = false;
            }
        }

        private static bool canUse(PlayerControllerB __instance)
        {
            Ray interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
            Physics.Raycast(interactRay, out RaycastHit hit, __instance.grabDistance, interactableObjectsMask);
            GrabbableObject currentlyGrabbingObject = hit.collider?.GetComponent<GrabbableObject>();
            if (currentlyGrabbingObject != null)
            {
                var objectName = GeneralUtil.simplifyObjectNames(currentlyGrabbingObject.name);
                if (objectName == "RagdollGrabbableObject")
                    return false;
            }

            return true;
        }
    }
}
