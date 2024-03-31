using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.OneHitShovel.CustomRpc;

namespace OPJosMod.OneHitShovel.Patches
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
        private static bool killPlayerPatch(PlayerControllerB __instance)
        {
            mls.LogMessage("dont kill player, testing");
            return false;
        }
    }
}
