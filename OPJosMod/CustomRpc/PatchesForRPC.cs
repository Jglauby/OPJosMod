﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.TheFlash;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMod.TheFlash.CustomRpc
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
        private static float lastRecieved = Time.time;
        private static float messageWaitTime = 0.1f;

        [HarmonyPatch("AddPlayerChatMessageClientRpc")]
        [HarmonyPrefix]
        private static void addPlayerChatMessageClientRpcPatch(ref string chatMessage, ref int playerId)
        {
            if (MessageCodeUtil.stringContainsMessageCode(chatMessage) && Time.time - lastRecieved > messageWaitTime)
            {
                lastRecieved = Time.time;
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
                GlobalVariables.ModActivated = true;
                string message = MessageTaskUtil.GetCode(MessageTasks.ModActivated) + PatchesForRPC.mls.SourceName;
                RpcMessage rpcMessage = new RpcMessage(message, (int)__instance.localPlayerController.playerClientId, MessageCodes.Request);
                RpcMessageHandler.SendRpcMessage(rpcMessage);
            }
        }
    }
}