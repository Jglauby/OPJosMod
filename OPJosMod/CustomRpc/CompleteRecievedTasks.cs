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
                GlobalVariables.ModActivated = true;
            }                
        }

        public static void PlayerJumped(string playerName)
        {
            HUDManager.Instance.DisplayTip(playerName, "has jumped!");
        }
    }
}
