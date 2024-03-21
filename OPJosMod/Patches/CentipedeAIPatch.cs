using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace OPJosMod.LagJutsu.Patches
{
    [HarmonyPatch(typeof(CentipedeAI))]
    internal class CentipedeAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool onCollideWithPlayerPatch(CentipedeAI __instance, ref Collider other)
        {
            if (PlayerControllerBPatch.godMode && !__instance.isEnemyDead)
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                if (StartOfRound.Instance.localPlayerController.playerClientId == component.playerClientId)
                {
                    //mls.LogMessage("centipede collide with player patch hit");
                    PlayerControllerBPatch.teleportPlayerBack();
                    return false;
                }
            }

            return true;
        }
    }
}
