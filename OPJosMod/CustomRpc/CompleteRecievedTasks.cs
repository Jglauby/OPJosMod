using BepInEx.Logging;
using GameNetcodeStuff;
using static Unity.Netcode.FastBufferWriter;
using Unity.Netcode;
using UnityEngine;

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

        public static void RevivePlayer(string position)
        {
            Vector3 revivedPosition = GeneralUtil.StringToVector3(position);
            GeneralUtil.RevivePlayer(revivedPosition);
        }
    }
}
