using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.Patches
{
    [HarmonyPatch(typeof(JetpackItem))]
    internal class JetpackItemPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static bool Update(JetpackItem __instance)
        {
            if (__instance.playerHeldBy == null)
            {
                return true;
            }

            if (__instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                __instance.playerHeldBy.takingFallDamage = false;
                __instance.playerHeldBy.averageVelocity = 0f;//make game think u arent going fast so u dont hurt yourself
                __instance.itemProperties.requiresBattery = false; // make it have infinite battery
                __instance.jetpackBeepsAudio.volume = 0f;
            }

            FieldInfo rayHitField = typeof(JetpackItem).GetField("rayHit", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo forcesField = typeof(JetpackItem).GetField("forces", BindingFlags.NonPublic | BindingFlags.Instance);
            RaycastHit rayHit = (RaycastHit)rayHitField.GetValue(__instance);
            Vector3 forces = (Vector3)forcesField.GetValue(__instance);
            if (!__instance.playerHeldBy.isPlayerDead && Physics.Raycast(__instance.playerHeldBy.transform.position, forces, out rayHit, 25f, StartOfRound.Instance.allPlayersCollideWithMask, QueryTriggerInteraction.Ignore) 
                && forces.magnitude - rayHit.distance > 50f && rayHit.distance < 4f)
            {
                //playerHeldBy.KillPlayer(forces, spawnBody: true, CauseOfDeath.Gravity);
                return false;
            }

            return true;
        }
    }
}
