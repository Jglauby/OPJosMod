using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

       
    }
}
