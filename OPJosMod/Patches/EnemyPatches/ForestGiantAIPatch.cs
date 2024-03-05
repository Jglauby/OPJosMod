using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.OneHitShovel.Patches
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
        private static void onCollideWithPlayerPatch(ForestGiantAI __instance)
        {
            mls.LogMessage("giant colided with player");
            __instance.isEnemyDead = true;
        }
    }
}
