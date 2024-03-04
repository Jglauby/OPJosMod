using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.HideNSeek.Config;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPrefix]
        private static void hitShovelPatch(Shovel __instance)
        {
            __instance.shovelHitForce = 30;
        }
    }
}
