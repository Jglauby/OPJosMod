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

        private static float lastHealedAt = Time.time;

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (__instance.health < 100 && Time.time - lastHealedAt > ConfigVariables.healFrequency)
            {
                HealPlayer(__instance);
            }
        }

        private static void HealPlayer(PlayerControllerB player)
        {
            if (!player.isPlayerDead)
            {
                lastHealedAt = Time.time;
                int healthToAddCritical = 1;
                int healthToAdd = ConfigVariables.healAmount;

                if (player.criticallyInjured || player.health <= 10)
                {
                    player.health += healthToAddCritical;
                }
                else
                {
                    if (player.health + healthToAdd >= 100)
                    {
                        int healthHealed = 100 - player.health;
                        player.health = 100;
                        player.DamagePlayerServerRpc(-healthHealed, player.health);
                        mls.LogMessage($"first if, healthHealed:{healthHealed}, player helath:{player.health}");
                    }
                    else
                    {
                        player.health += healthToAdd;
                        player.DamagePlayerServerRpc(-healthToAdd, player.health);
                        mls.LogMessage($"second if, healthToAdd:{healthToAdd}, player helath:{player.health}");
                    }
                    HUDManager.Instance.UpdateHealthUI(player.health, false);
                }

                mls.LogMessage($"updated health to:{player.health} at: {Time.time}");
            }
        }
    }
}
