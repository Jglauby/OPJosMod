using BepInEx.Logging;
using HarmonyLib;

namespace OPJosMod.OPClientSide.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void updatePatch(StartMatchLever __instance)
        {

        }    
    }
}
