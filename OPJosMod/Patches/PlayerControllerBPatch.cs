using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.HealthRegen.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        private static int healRatePosition = 0;
        private static int healRate = 4500;
        private static int healAmount = 5;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (__instance.health < 100 && healRatePosition >= healRate)
            {
                if (__instance.criticallyInjured || __instance.health <= 10)//take a bit longer to get out of that state
                {
                    __instance.health += 1;
                }
                else
                {
                    if (__instance.health + healAmount >= 100)//full hp
                    {
                        __instance.health = 100;
                    }
                    else
                    {
                        __instance.health += healAmount;
                    }
                    HUDManager.Instance.UpdateHealthUI(__instance.health, false);
                }           
                healRatePosition = 0;
                mls.LogMessage("updated health to:" + __instance.health);
            }
            healRatePosition += 1;
        }
    }
}
