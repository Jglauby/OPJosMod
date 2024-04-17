using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace OPJosMod.TheFlash.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatch : MonoBehaviour
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPostfix]
        static void TeleportPlayerPatch()
        {
            if (!GlobalVariables.ModActivated)
                return;

            PlayerControllerBPatch.InitializeNaveMeshForPlayer();
        }      
    }
}
