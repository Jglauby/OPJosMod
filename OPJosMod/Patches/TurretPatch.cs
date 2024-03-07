using BepInEx.Logging;
using HarmonyLib;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(Turret))]
    internal class TurretPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void startPatch(Turret __instance)
        {
            __instance.turretActive = false;
        }
    }
}
