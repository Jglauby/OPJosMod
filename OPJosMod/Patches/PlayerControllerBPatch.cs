using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.ReviveCompany.CustomRpc;
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

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            //mls.LogMessage($"Constants.ModActivated:{GlobalVariables.ModActivated}");
        }

        [HarmonyPatch("Interact_performed")]
        [HarmonyPrefix]
        private static bool interact_performedPatch(PlayerControllerB __instance)
        {
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
            if (!canUse(__instance) && __instance.cursorTip.text != "")
            {
                if (((ButtonControl)Keyboard.current[Key.E]).isPressed)//E is pressed
                {
                    if (StartedRevive == false)
                    {
                        StartedRevive = true;
                        StartedReviveAt = Time.time;
                    }
                    else if (Time.time - StartedReviveAt > 7f)
                    {
                        //send revive message!
                        CompleteRecievedTasks.RevivePlayer(__instance.transform.position.ToString());
                        RpcMessage rpcMessage = new RpcMessage(MessageTasks.RevivePlayer, __instance.transform.position.ToString(), (int)__instance.playerClientId, MessageCodes.Request);
                        RpcMessageHandler.SendRpcMessage(rpcMessage);
                    }

                    __instance.cursorTip.text = "Reviving!";
                }
                else
                {
                    __instance.cursorTip.text = "Hold E to revive!";
                    StartedRevive = false;
                }
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
