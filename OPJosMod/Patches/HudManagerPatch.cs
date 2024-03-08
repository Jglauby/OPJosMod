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

        [HarmonyPatch("AddPlayerChatMessageServerRpc")]
        [HarmonyPrefix]
        private static bool addPlayerChatMessageServerRpcPatch(ref string chatMessage, ref int playerId)
        {
            //checks if it is a custom rpc call
            if (MessageCodeUtil.stringContainsMessageCode(chatMessage))
            {
                RpcMessageHandler.ReceiveRpcMessage(chatMessage, playerId);
                return false;
            }        

            return true;
        }
    }
}
