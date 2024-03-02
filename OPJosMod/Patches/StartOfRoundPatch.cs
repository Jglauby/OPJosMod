using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.Runtime.CompilerServices;
using Unity.Netcode;

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

        private static bool isHost = false;

        [HarmonyPatch("openingDoorsSequence")]
        [HarmonyPostfix]
        static void patchOpeningDoorsSequence(StartOfRound __instance)
        {
            mls.LogMessage("level actually began");
            setupLevel();

            //if the round starts and you arent set as seeker, then you didnt pull the lever and you should be a hider
            if (!PlayerControllerBPatch.isSeeker)
                PlayerControllerBPatch.SetupHider();
        }

        private static void setupLevel()
        {
            isHost = GameNetworkManager.Instance.isHostingGame;

            if(isHost)
            {
                //remove/don't spawn all enemies on the map
                    //may be better if they did spawn but then died, potential to hide by dead bodies


                //ensure new enemies don't spawn?
            }
        }
    }
}
