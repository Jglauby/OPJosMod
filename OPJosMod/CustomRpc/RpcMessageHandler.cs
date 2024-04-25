using BepInEx.Logging;
using System;
using System.Reflection;
using UnityEngine;

namespace OPJosMod.HideNSeek.CustomRpc
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
        private static float messageWaitTime = 0.5f;

        public static void SendRpcMessage(RpcMessage message)
        {
            if (Time.time - lastSentTime > messageWaitTime || message != lastSentMessage)
            {
                mls.LogMessage($"Sending rpc message: {message.getMessageWithCode()}, user:{message.FromUser}");
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
            else
            {
                mls.LogError($"didnt' send message as it was too soon and was the same message as last message {message.getMessageWithCode()}");
            }
        }

        public static void ReceiveRpcMessage(string message, int user)
        {
            if (user != (int)StartOfRound.Instance.localPlayerController.playerClientId)
            {
                mls.LogMessage($"recieved message: {message}, user:{user}");
                var decodedMessage = MessageCodeUtil.returnMessageWithoutCode(message);
                if (message.Contains(MessageCodeUtil.GetCode(MessageCodes.Request)))
                {
                    MessageTasks task = MessageTaskUtil.getMessageTask(decodedMessage);
                    string taskMessage = MessageTaskUtil.getMessageWithoutTask(decodedMessage);

                    SendRpcResponse(task, taskMessage);
                    handleTask(task, taskMessage);
                }
                else if (message.Contains(MessageCodeUtil.GetCode(MessageCodes.Response)))
                {
                    mls.LogMessage($"got the response that the other clients recieved this message: {message}");
                }
            }
        }

        public static void SendRpcResponse(MessageTasks task, string message)
        {
            var responseMessage = new RpcMessage(task, message, (int)StartOfRound.Instance.localPlayerController.playerClientId, MessageCodes.Response);
            SendRpcMessage(responseMessage);
        }

        private static void handleTask(MessageTasks task, string message)
        {
            try
            {
                switch (task)
                {
                    case MessageTasks.StartedSeeking:
                        CompleteRecievedTasks.SeekingStarted(message);
                        break;
                    case MessageTasks.MakePlayerWhistle:
                        CompleteRecievedTasks.MakePlayerWhistle(message);
                        break;
                    case MessageTasks.ErrorNoTask:
                        mls.LogError("got an error task");
                        break;
                }
            }
            catch (Exception e)
            {
                mls.LogError($"failed when handling rpc task, {e}");
            }
        }
    }
}
