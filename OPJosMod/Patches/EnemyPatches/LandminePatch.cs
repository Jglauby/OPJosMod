using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using UnityEngine;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        private static bool onTriggerEnterPatch(ref Collider other)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if (other.CompareTag("Player"))
                {
                    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                    if (!(component != GameNetworkManager.Instance.localPlayerController) && component != null && !component.isPlayerDead)
                    {
                        mls.LogMessage("ghost stepped on mine, do nothing");
                        return false;
                    }
                }
            }

            return true;
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        private static bool onTriggerExitPatch(ref Collider other)
        {
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
            {
                if (other.CompareTag("Player"))
                {
                    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                    if (component != null && !component.isPlayerDead && !(component != GameNetworkManager.Instance.localPlayerController))
                    {
                        mls.LogMessage("ghost stepped off mine, do nothing");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
