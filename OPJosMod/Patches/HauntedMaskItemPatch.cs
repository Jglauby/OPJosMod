using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.LagJutsu.Utils;
using UnityEngine;

namespace OPJosMod.LagJutsu.Patches
{
    [HarmonyPatch(typeof(HauntedMaskItem))]
    internal class HauntedMaskItemPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("BeginAttachment")]
        [HarmonyPrefix]
        static bool beginAttachmentPatch(HauntedMaskItem __instance)
        {
            if (PlayerControllerBPatch.godMode)
            {
                __instance.AttachServerRpc();
                UnityEngine.Object.Destroy(__instance.gameObject);
                return false;
            }

            return true;
        }
    }
}
