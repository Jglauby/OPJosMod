using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.OneHitShovel.CustomRpc;
using OPJosMod.OneHitShovel.CustomRpcs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.OneHitShovel.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static bool sendingMessageStarted = false;

        [HarmonyPatch("Jump_performed")]
        [HarmonyPrefix]
        private static void jump_performed(PlayerControllerB __instance)
        {
            mls.LogMessage("you jumped!");

            if (!sendingMessageStarted)
            {
                var rpcMessage = new RpcMessage("other user jumped!", (int)StartOfRound.Instance.localPlayerController.playerClientId, MessageCodes.Request);
                __instance.StartCoroutine(sendMessage(rpcMessage));
                sendingMessageStarted = true;
            }
        }

        private static IEnumerator sendMessage(RpcMessage message)
        {
            yield return new WaitForSeconds(3f);
            RpcMessageHandler.SendRpcMessage(message);
            sendingMessageStarted = false;
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        private static bool killPlayerPatch(PlayerControllerB __instance)
        {
            mls.LogMessage("dont kill player, testing");
            return false;
        }
    }
}
