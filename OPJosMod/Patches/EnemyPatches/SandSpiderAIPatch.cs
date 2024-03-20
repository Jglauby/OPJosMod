using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using OPJosMod.SupahNinja.Utils;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(SandSpiderAI))]
    internal class SandSpiderAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("TriggerChaseWithPlayer")]
        [HarmonyPrefix]
        static bool triggerChaseWithPlayerPatch(ref PlayerControllerB playerScript)
        {
            if (StartOfRound.Instance.localPlayerController.playerClientId == playerScript.playerClientId && GeneralUtils.playerIsCrouching())
            {
                mls.LogMessage("spider supposed to trigger chase with player");
                return false;
            }

            return true;
        }
    }
}
