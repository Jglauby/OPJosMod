using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        private static int healAmount = 5;
        private static bool isHealing = false;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (__instance.health < 100 && !isHealing)
            {
                __instance.StartCoroutine(HealPlayer(__instance, 10f));
            }
        }

        private static IEnumerator HealPlayer(PlayerControllerB player, float frequency)
        {
            isHealing = true;

            yield return new WaitForSeconds(frequency);

            int healthToAddCritical = 1;
            int healthToAdd = 4;

            if (player.criticallyInjured || player.health <= 10)
            {
                player.health += healthToAddCritical;
            }
            else
            {
                if (player.health + healthToAdd >= 100)
                {
                    player.health = 100;
                }
                else
                {
                    player.health += healthToAdd;
                }
                HUDManager.Instance.UpdateHealthUI(player.health, false);
            }

            mls.LogMessage($"updated health to:{player.health} at: {Time.time}");

            isHealing = false;
        }
    }
}
