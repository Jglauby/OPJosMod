using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.InputSystem.InputRemoting;

namespace OPJosMod.ReviveCompany.CustomRpc
{
    public static class ResponseHandler
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static float LastSendTime = 0;
        private static RpcMessage ResendMessageMessage = null;
        private static bool StartWaitForResend = false;
        private static int TotalResponses = 0;
        private static int TotalSentMessages = 0;
        public static void RecievedResponse(MessageTasks messageTask)
        {
            if (messageTask != MessageTasks.RevivePlayer)
                return; //dont care about reciving non revie player messages

            if (StartOfRound.Instance == null || StartOfRound.Instance.allPlayerScripts == null)
                return; //start of round instance isn't seutp dont crash, sholdnt be possible but *shrug*

            TotalResponses++;
        }

        public static void SentMessageNeedResponses(RpcMessage message)
        {
            LastSendTime = Time.time;
            ResendMessageMessage = message;
            StartWaitForResend = true;
            TotalResponses = 0;
            TotalSentMessages = 0;
        }

        public static void CalculateIfShouldResend()
        {
            if (ResendMessageMessage == null || ResendMessageMessage.Task != MessageTasks.RevivePlayer)           
                return; //should not resend anything but revive message

            if (StartWaitForResend && Time.time - LastSendTime > 2f)
            {
                if (TotalSentMessages > 4)
                {
                    mls.LogError("No longer retrying message send as it has already sent 5 extra times.");
                    LastSendTime = 0;
                    StartWaitForResend = false;
                    TotalResponses = 0;
                    TotalSentMessages = 0;
                    return;
                }

                //if has enough responses for all players, just checks number of responses could be form the same person
                var playerCount = RoundManager.Instance.playersManager.allPlayerScripts.Where(x => !x.playerUsername.Contains("Player")).ToList().Count();
                mls.LogMessage($"TotalResponses:{TotalResponses}, playerCount:{playerCount}");
                if (TotalResponses > playerCount - 2 || playerCount - 2 < 0)
                {
                    LastSendTime = 0;
                    StartWaitForResend = false;
                    TotalResponses = 0;
                    TotalSentMessages = 0;
                    return;
                }

                mls.LogMessage("resending revive message as dont have enough responses");
                TotalSentMessages++;
                LastSendTime = Time.time;
                RpcMessageHandler.SendRpcMessage(ResendMessageMessage);

                ResendMessageMessage.Task = MessageTasks.TurnOffGhostMode;
                RpcMessageHandler.SendRpcMessage(ResendMessageMessage);
                ResendMessageMessage.Task = MessageTasks.RevivePlayer;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatchResponseHandler
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void patchUpdate(PlayerControllerB __instance)
        {
            if (!GlobalVariables.ModActivated)
                return;

            if (__instance.IsOwner)
            {
                ResponseHandler.CalculateIfShouldResend();
            }

            return;
        }
    }
}
