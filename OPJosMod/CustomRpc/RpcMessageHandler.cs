﻿using BepInEx.Logging;
using OPJosMod.OneHitShovel.CustomRpcs;
using OPJosMod.OneHitShovel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.OneHitShovel.CustomRpc
{
    public static class RpcMessageHandler
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void SendRpcMessage(RpcMessage message)
        {
            HUDManager hudManagerInstance = HUDManager.Instance;
            if (hudManagerInstance != null)
            {
                MethodInfo method = typeof(HUDManager).GetMethod("AddPlayerChatMessageServerRpc", BindingFlags.Instance | BindingFlags.NonPublic);

                if (method != null)
                {
                    object[] parameters = new object[] { message.getMessageWithCode(), message.FromUser };
                    method.Invoke(hudManagerInstance, parameters);
                }
                else
                {
                    mls.LogError("AddPlayerChatMessageServerRpc method not found in HUDManager class.");
                }
            }
            else
            {
                mls.LogError("HUDManager.Instance is null.");
            }
        }

        public static void ReceiveRpcMessage(string message, int user) 
        {
            if (user == (int)StartOfRound.Instance.localPlayerController.playerClientId)
                return;

            var decodedMessage = MessageCodeUtil.returnMessageWithoutCode(message);
            if (message.Contains(MessageCodeUtil.GetCode(MessageCodes.Request)))
            {               
                mls.LogMessage(decodedMessage + $" user {user}");

                SendRpcResponse(decodedMessage);
            }
            else if (message.Contains(MessageCodeUtil.GetCode(MessageCodes.Response)))
            {
                mls.LogMessage("got the response that the other clients recieved this message");
            }
        }

        public static void SendRpcResponse(string message)
        {
            var responseMessage = new RpcMessage(message, (int)StartOfRound.Instance.localPlayerController.playerClientId, MessageCodes.Response);
            SendRpcMessage(responseMessage);
        }
    }
}
