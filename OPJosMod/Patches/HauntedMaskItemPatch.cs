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

        [HarmonyPatch("FinishAttaching")]
        [HarmonyPrefix]
        static bool finishAttachingPatchPRE(HauntedMaskItem __instance)
        {
            if (PlayerControllerBPatch.godMode)
            {
                var heldByPlayer = ReflectionUtils.GetFieldValue<PlayerControllerB>(__instance, "previousPlayerHeldBy");
                if (heldByPlayer != null && heldByPlayer.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                {
                    ReflectionUtils.SetFieldValue(__instance, "finishedAttaching", true);
                    ReflectionUtils.InvokeMethod(__instance, "CancelAttachToPlayerOnLocalClient", new object[] { });
                    return false;
                }
            }
        
            return true;
        }
    }
}
