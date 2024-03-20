using BepInEx.Logging;
using HarmonyLib;
using OPJosMod;
using OPJosMod.HideNSeek.Config;

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
            if(__instance.flashlightTypeID == (int)FlashlightTypes.NormalFlashlight)
            {
                __instance.itemProperties.requiresBattery = false;
                __instance.flashlightBulb.intensity = __instance.flashlightBulb.intensity / ConfigVariables.smallFlashlightPower;
            }
            else if (__instance.flashlightTypeID == (int)FlashlightTypes.ProFlashlight)
            {
                //mls.LogMessage($"pro flashlight weight: {__instance.itemProperties.weight}");
                //default pro flashlight weight = 1.05f
                __instance.itemProperties.weight = 1.05f * ConfigVariables.proFlashlightWeightMultiplier;

                //mls.LogMessage($"pro flashlight battery usage: {__instance.itemProperties.batteryUsage}");
                //defautl pro flashlight battery usage is 300
                __instance.itemProperties.batteryUsage = 300 * ConfigVariables.proFlashlightBatteryUsageMultiplier;
            }
        }
    }
}
