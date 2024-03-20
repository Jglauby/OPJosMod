using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using OPJosMod.SupahNinja.Utils;
using UnityEngine;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(ForestGiantAI))]
    internal class ForestGiantAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool onCollideWithPlayerPatch(ref Collider other)
        {
            if (GeneralUtils.playerIsCrouching())
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                if (StartOfRound.Instance.localPlayerController.playerClientId == component.playerClientId)
                {
                    mls.LogMessage("forest giant collide with player patch hit");
                    return false;
                }
            }

            return true;
        }
    }
}
