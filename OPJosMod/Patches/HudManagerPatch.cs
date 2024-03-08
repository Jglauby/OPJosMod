using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.OneHitShovel.CustomRpc;
using OPJosMod.OneHitShovel.CustomRpcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.OneHitShovel.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static float lastRecieved = Time.time;
        private static float messageWaitTime = 1f;

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
    }
}
