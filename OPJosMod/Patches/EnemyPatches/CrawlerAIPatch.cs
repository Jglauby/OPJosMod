using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.GhostMode.Patches;

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
            if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou 
                && EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                __instance.currentBehaviourStateIndex = 0;
            }        
        }
    }
}
