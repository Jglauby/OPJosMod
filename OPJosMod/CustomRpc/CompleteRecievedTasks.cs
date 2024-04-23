using BepInEx.Logging;
using GameNetcodeStuff;
using static Unity.Netcode.FastBufferWriter;
using Unity.Netcode;
using UnityEngine;
using OPJosMod.GhostMode.Patches;

namespace OPJosMod.GhostMode.CustomRpc
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

        public static void TurnOffGhostMode(string clientID)
        {
            if (int.TryParse(clientID, out var clientIDInt))
            {
                if ((int)StartOfRound.Instance.localPlayerController.playerClientId == clientIDInt)
                {
                    //teleport to dead body
                    mls.LogMessage("Revived on Other Client, turning off ghost mode.");
                    var player = StartOfRound.Instance.localPlayerController;
                    player.TeleportPlayer(player.deadBody.transform.position);

                    PlayerControllerBPatch.resetGhostModeVars(player);
                }
            }          
        }
    }
}
