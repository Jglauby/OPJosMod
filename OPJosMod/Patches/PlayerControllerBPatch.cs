using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.GodMode.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        static void patchKillPlayer(PlayerControllerB __instance)
        {
            mls.LogMessage("should've died but didn't");
            throw new Exception("actually don't kill");
        }
    }
}
