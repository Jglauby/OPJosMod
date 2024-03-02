using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("openingDoorsSequence")]
        [HarmonyPostfix]
        static void patchOpeningDoorsSequence(StartOfRound __instance)
        {
            //once the level actually begins this patch should be hit

            mls.LogMessage("level actually began");
            PlayerControllerBPatch.SetupSeeker();
        }
    }
}
