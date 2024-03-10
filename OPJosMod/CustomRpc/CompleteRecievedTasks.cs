using BepInEx.Logging;
using GameNetcodeStuff;
using OPJosMode.HideNSeek.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.HideNSeek.CustomRpc
{
    public static class CompleteRecievedTasks
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void SeekingStarted()
        {
            if (PlayerControllerBPatch.isHider)
            {
                HUDManagerPatch.CustomDisplayTip("Careful!", "Seeker is on their way!");
            }
        }

        public static void MakePlayerWhistle(string playerIdString)
        {
            int playerClientId = int.Parse(playerIdString);
            mls.LogMessage($"make player{playerClientId} whistle");

            if((int)StartOfRound.Instance.localPlayerController.playerClientId == playerClientId)
            {
                //FORCE U TO WHISTLE!!!
            }
        }
    }
}
