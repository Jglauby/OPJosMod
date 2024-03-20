using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using OPJosMod.SupahNinja.Utils;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(CentipedeAI))]
    internal class CentipedeAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool updatePrePatch(CentipedeAI __instance)
        {
            if (GeneralUtils.playerIsCrouching())
            {
                //do nothing if ghost is closest to centipede and its hanging from ceiling
                if (EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId
                    && __instance.currentBehaviourStateIndex == 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
