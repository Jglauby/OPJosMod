﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.ModNameHere;
using OPJosMod.MODNAMEHERE.CustomRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.Patches//OPJosMod.MODNameHere.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            //mls.LogMessage($"Constants.ModActivated:{GlobalVariables.ModActivated}");
        }

        [HarmonyPatch("PlayerJump")]
        [HarmonyPostfix]
        static void patchPlayerJump(PlayerControllerB __instance)
        {
            if (__instance.playerClientId == GameNetworkManager.Instance.localPlayerController.playerClientId)
            {
                RpcMessage rpcMessage = new RpcMessage(MessageTasks.PlayerJumped, __instance.playerUsername, (int)__instance.playerClientId, MessageCodes.Request);
                RpcMessageHandler.SendRpcMessage(rpcMessage);
            }
        }
    }
}
