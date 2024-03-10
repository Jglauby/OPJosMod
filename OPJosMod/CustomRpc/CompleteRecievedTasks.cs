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
        public static void SeekingStarted()
        {
            if (PlayerControllerBPatch.isHider)
            {
                HUDManagerPatch.CustomDisplayTip("Careful!", "Seeker is on their way!");
            }
        }
    }
}
