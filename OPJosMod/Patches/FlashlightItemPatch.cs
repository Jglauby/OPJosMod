using BepInEx.Logging;
using HarmonyLib;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(FlashlightItem))]
    internal class FlashlightItemPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void startPatch(FlashlightItem __instance)
        {
            __instance.itemProperties.requiresBattery = false;
            __instance.flashlightBulb.intensity = __instance.flashlightBulb.intensity / 5;
        }
    }
}
