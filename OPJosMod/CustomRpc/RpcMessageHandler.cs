using BepInEx.Logging;
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

        private static float lastSentTime = Time.time;
        private static float messageWaitTime = 1f;

        public static void SendRpcMessage(RpcMessage message)
        {
            if (Time.time - lastSentTime > messageWaitTime)
            {
                lastSentTime = Time.time;
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
            if (user == (int)StartOfRound.Instance.localPlayerController.playerClientId)
                return;

            var decodedMessage = MessageCodeUtil.returnMessageWithoutCode(message);
            if (message.Contains(MessageCodeUtil.GetCode(MessageCodes.Request)))
            {
                MessageTasks task = MessageTaskUtil.getMessageTask(decodedMessage);
                handleTask(task);

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

        private static void handleTask(MessageTasks task)
        {
            switch (task)
            {
                case MessageTasks.StartedSeeking:
                    CompleteRecievedTasks.SeekingStarted();
                    break;
                case MessageTasks.ErrorNoTask:
                    mls.LogError("got an error task");
                    break;
            }
        }
    }
}
