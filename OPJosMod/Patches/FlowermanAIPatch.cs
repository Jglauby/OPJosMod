using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace OPJosMod.LagJutsu.Patches
{
    [HarmonyPatch(typeof(FlowermanAI))]
    internal class FlowermanAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool onCollideWithPlayerPatch(FlowermanAI __instance, ref Collider other)
        {
            if (PlayerControllerBPatch.godMode && !__instance.isEnemyDead)
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                if (StartOfRound.Instance.localPlayerController.playerClientId == component.playerClientId)
                {
                    //mls.LogMessage("flowerman collide with player patch hit");
                    PlayerControllerBPatch.teleportPlayerBack();
                    return false;
                }
            }

            return true;
        }
    }
}
