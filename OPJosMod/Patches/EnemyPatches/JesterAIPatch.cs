using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Patches;
using OPJosMod.SupahNinja.Utils;

namespace OPJosMod.SupahNinja.Enemy.Patches
{
    [HarmonyPatch(typeof(JesterAI))]
    internal class JesterAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void updatePatch(JesterAI __instance)
        {
            if (GeneralUtils.playerIsCrouching())
            {
                if (__instance.currentBehaviourStateIndex == 2 || __instance.currentBehaviourStateIndex == 1)//if currently chasing a player/ or winding
                {
                    if (EnemyAIPatch.ghostOnlyPlayerInFacility())
                    { 
                        if (EnemyAIPatch.getClosestPlayerIncludingGhost(__instance).playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
                        {
                            mls.LogMessage("swaping jester back to state 0");
                            __instance.SwitchToBehaviourState(1);
                        }
                    }
                }
            }
        }
    }
}
