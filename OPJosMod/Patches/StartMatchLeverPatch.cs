using BepInEx.Logging;
using HarmonyLib;

namespace OPJosMod.GhostMode.Patches
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
            if (PlayerControllerBPatch.isGhostMode && ConfigVariables.OPness != OPnessModes.Unrestricted)
            {
                __instance.triggerScript.hoverTip = "Can't use this as ghost!";
                __instance.triggerScript.interactable = false;
            }
        }    
    }
}
