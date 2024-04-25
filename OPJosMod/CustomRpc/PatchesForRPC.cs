using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace OPJosMod.GhostMode.CustomRpc
{
    public static class PatchesForRPC
    {
        public static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }
    }

    [HarmonyPatch(typeof(HUDManager))]
    static class HUDManagerPatchForRPC
    {
        private static float lastRecievedAt = Time.time;
        private static string lastRecievedMessage = null;
        private static float messageWaitTime = 0.5f;

        [HarmonyPatch("AddPlayerChatMessageClientRpc")]
        [HarmonyPrefix]
        private static void addPlayerChatMessageClientRpcPatch(ref string chatMessage, ref int playerId)
        {
            var rawMessage = MessageCodeUtil.returnMessageNoSeperators(chatMessage);
            if (MessageCodeUtil.stringContainsMessageCode(chatMessage) &&
                (Time.time - lastRecievedAt > messageWaitTime || rawMessage != lastRecievedMessage) &&
                (playerId != (int)GameNetworkManager.Instance.localPlayerController.playerClientId))
            {
                lastRecievedAt = Time.time;
                lastRecievedMessage = rawMessage;
                RpcMessageHandler.ReceiveRpcMessage(chatMessage, playerId);
            }
        }

        [HarmonyPatch("AddChatMessage")]
        [HarmonyPrefix]
        private static bool addChatMessagePatch(ref string chatMessage)
        {
            //keep chat from showing up if it was a just being used for a rpc call
            if (MessageCodeUtil.stringContainsMessageCode(chatMessage))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StartOfRound))]
    static class StartOfRoundPatchForRPC
    {
        [HarmonyPatch("OnClientConnect")]
        [HarmonyPostfix]
        static void patchOnClientConnect(StartOfRound __instance)
        {
            __instance.StartCoroutine(activateModForOthers(__instance));
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patchStart(StartOfRound __instance)
        {
            if (__instance.IsHost)
                GlobalVariables.ModActivated = true;
        }

        [HarmonyPatch("OnLocalDisconnect")]
        [HarmonyPostfix]
        static void patchOnLocalDisconnect(StartOfRound __instance)
        {
            GlobalVariables.ModActivated = false;
        }

        private static IEnumerator activateModForOthers(StartOfRound __instance)
        {
            yield return new WaitForSeconds(3f);
            if (__instance.IsHost)
            {
                //GlobalVariables.ModActivated = true;
                //RpcMessage rpcMessage = new RpcMessage(MessageTasks.ModActivated, PatchesForRPC.mls.SourceName, (int)__instance.localPlayerController.playerClientId, MessageCodes.Request);
                //RpcMessageHandler.SendRpcMessage(rpcMessage);
            }
        }
    }
}
