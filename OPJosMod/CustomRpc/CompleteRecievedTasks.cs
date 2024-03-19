using BepInEx.Logging;
using GameNetcodeStuff;
using OPJosMod.HideNSeek.Config;
using OPJosMode.HideNSeek.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.HideNSeek.CustomRpc
{
    public static class CompleteRecievedTasks
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void SeekingStarted(string playerIdString)
        {
            if (PlayerControllerBPatch.isHider)
            {
                if (ConfigVariables.hiderItem == BuyableItems.None)
                    HUDManagerPatch.CustomDisplayTip("Careful!", "Seeker is on their way!");
                else
                    HUDManagerPatch.CustomDisplayTip("Seeker is on their way!", "an item dropped at your feet");
            }

            GeneralUtil.spawnHiderItem(int.Parse(playerIdString));
        }

        public static void MakePlayerWhistle(string playerIdString)
        {
            int playerClientId = int.Parse(playerIdString);
            mls.LogMessage($"make player{playerClientId} whistle");

            var fartingPlayer = StartOfRound.Instance.allPlayerScripts[playerClientId];
            PlaySounds.PlayFart(fartingPlayer);

            if ((int)StartOfRound.Instance.localPlayerController.playerClientId == playerClientId)
            {
                //you are the one who is makign noise
                mls.LogMessage("seeker made u make noise");
            }
        }
    }
}
