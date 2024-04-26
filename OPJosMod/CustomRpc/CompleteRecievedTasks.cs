using BepInEx.Logging;
using GameNetcodeStuff;
using static Unity.Netcode.FastBufferWriter;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using System;

namespace OPJosMod.ReviveCompany.CustomRpc
{
    public static class CompleteRecievedTasks
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void ModActivated(string modName)
        {
            if (mls.SourceName == modName)
            {
                mls.LogMessage("Mod Activated");
                GlobalVariables.ModActivated = true;
            }                
        }

        public static void RevivePlayer(string playerIdString)
        {
            if (int.TryParse(playerIdString, out var playerId))
            {
                GeneralUtil.RevivePlayer(playerId);
            }
            else
            {
                mls.LogError($"Error: Invalid player ID '{playerIdString}' did not revive");
            }
        }
    }
}
