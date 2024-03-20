using BepInEx.Logging;
using HarmonyLib;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemyPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        //[HarmonyPatch("Update")]
        //[HarmonyPrefix]
        //private static void updatePatch(MaskedPlayerEnemy __instance)
        //{
        //    if (PlayerControllerBPatch.isGhostMode && !ConfigVariables.enemiesDetectYou)
        //    {
        //        //if closest
        //        __instance.currentBehaviourStateIndex = 0;
        //    }
        //}
    }
}
