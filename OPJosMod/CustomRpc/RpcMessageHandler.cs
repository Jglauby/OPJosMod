using BepInEx.Logging;
using System;
using System.Reflection;
using UnityEngine;

namespace OPJosMod.ReviveCompany.CustomRpc
{
    public static class RpcMessageHandler
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static RpcMessage lastSentMessage = null;
        private static float lastSentTime = Time.time;
        private static float messageWaitTime = 0.3f;

        public static void SendRpcMessage(RpcMessage message)
        {
            if (Time.time - lastSentTime > messageWaitTime || lastSentMessage != message)
            {
                lastSentTime = Time.time;
                lastSentMessage = message;
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
        }

        public static void ReceiveRpcMessage(string message, int user) 
        {
            if (user != (int)StartOfRound.Instance.localPlayerController.playerClientId)
            {
                var decodedMessage = MessageCodeUtil.returnMessageWithoutCode(message);
                if (message.Contains(MessageCodeUtil.GetCode(MessageCodes.Request)))
                {
                    MessageTasks task = MessageTaskUtil.getMessageTask(decodedMessage);
                    string taskMessage = MessageTaskUtil.getMessageWithoutTask(decodedMessage);

                    SendRpcResponse(task, decodedMessage);
                    handleTask(task, taskMessage);
                }
                else if (message.Contains(MessageCodeUtil.GetCode(MessageCodes.Response)))
                {
                    mls.LogMessage($"got the response that the other clients recieved this message:{message}");
                }
            }
        }

        public static void SendRpcResponse(MessageTasks task, string message)
        {
            try
            {
                var responseMessage = new RpcMessage(task, message, (int)StartOfRound.Instance.localPlayerController.playerClientId, MessageCodes.Response);
                SendRpcMessage(responseMessage);
            }
            catch (Exception e)
            {
                mls.LogError($"failed to send response message: {e}");
            }
        }

        private static void handleTask(MessageTasks task, string message)
        {
            try
            {
                switch (task)
                {
                    case MessageTasks.ModActivated:
                        CompleteRecievedTasks.ModActivated(message);
                        break;
                    case MessageTasks.RevivePlayer:
                        CompleteRecievedTasks.RevivePlayer(message);
                        break;
                    case MessageTasks.ErrorNoTask:
                        mls.LogError("got an error task");
                        break;
                }
            }
            catch(Exception e)
            {
                mls.LogError($"failed handlign rpc task: {e}");
            }
        }
    }
}
