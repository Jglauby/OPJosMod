using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Config;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        //[HarmonyPatch("Update")]
        //[HarmonyPrefix]
        //private static void updatePatch(TimeOfDay __instance)
        //{
        //    
        //}
    }
}
