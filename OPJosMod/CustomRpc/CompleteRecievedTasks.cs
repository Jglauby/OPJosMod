using BepInEx.Logging;
using OPJosMod.ModNameHere;

namespace OPJosMod.MODNAMEHERE.CustomRpc
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
                Constants.ModActivated = true;
            }                
        }

        public static void SeekingStarted(string playerIdString)
        {
            //if (PlayerControllerBPatch.isHider)
            //{
            //    if (ConfigVariables.hiderItem == BuyableItems.None)
            //        HUDManagerPatch.CustomDisplayTip("Careful!", "Seeker is on their way!");
            //    else
            //        HUDManagerPatch.CustomDisplayTip("Seeker is on their way!", "an item dropped at your feet");
            //}
            //
            //GeneralUtil.spawnHiderItem(int.Parse(playerIdString));
        }
    }
}
