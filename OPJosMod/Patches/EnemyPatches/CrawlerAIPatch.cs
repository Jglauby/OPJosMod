using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using OPJosMod.SupahNinja.Utils;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(CrawlerAI))]
    internal class CrawlerAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void updatePatch(CrawlerAI __instance)
        {
            if (GeneralUtils.playerIsCrouching()
                && EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                __instance.currentBehaviourStateIndex = 0;
            }        
        }
    }
}
