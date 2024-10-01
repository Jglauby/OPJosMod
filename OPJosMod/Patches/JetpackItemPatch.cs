using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;
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
            // Ensure the player is holding the jetpack
            if (__instance.playerHeldBy == null)
            {
                return true; // Skip if no player is holding the jetpack
            }

            if (__instance.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                // Disable fall damage for the player
                __instance.playerHeldBy.takingFallDamage = false;
                __instance.itemProperties.requiresBattery = false; // Infinite battery
                __instance.jetpackBeepsAudio.volume = 0f; // Mute beeping sounds

                // Access private fields via reflection
                FieldInfo jetpackPowerField = typeof(JetpackItem).GetField("jetpackPower", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo forcesField = typeof(JetpackItem).GetField("forces", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo jetpackActivatedField = typeof(JetpackItem).GetField("jetpackActivated", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo jetpackActivatedPreviousFrameField = typeof(JetpackItem).GetField("jetpackActivatedPreviousFrame", BindingFlags.NonPublic | BindingFlags.Instance);

                float jetpackPower = (float)jetpackPowerField.GetValue(__instance);
                Vector3 forces = (Vector3)forcesField.GetValue(__instance);
                bool jetpackActivated = (bool)jetpackActivatedField.GetValue(__instance);

                float deltaTime = Time.deltaTime;

                if (jetpackActivated)
                {
                    // Increase jetpack power when it's activated
                    jetpackPower = Mathf.Clamp(jetpackPower + deltaTime * __instance.jetpackAcceleration, 0f, 500f);
                }
                else
                {
                    // Decrease jetpack power when it's not activated
                    jetpackPower = Mathf.Clamp(jetpackPower - deltaTime * __instance.jetpackDeaccelleration, 0f, 1000f);

                    if (__instance.playerHeldBy.thisController.isGrounded)
                    {
                        jetpackPower = 0f; // Reset power if grounded
                    }
                }

                // Calculate the forces and apply them to the player
                forces = Vector3.Lerp(forces, Vector3.ClampMagnitude(__instance.playerHeldBy.transform.up * jetpackPower, 400f), deltaTime * 50f);

                // Reset forces if player is grounded or not using jetpack controls
                if (!__instance.playerHeldBy.jetpackControls || (jetpackPower > 10f && __instance.playerHeldBy.thisController.isGrounded))
                {
                    forces = Vector3.zero;
                }

                // Apply forces to the player's external forces
                if (__instance.playerHeldBy != null && !__instance.playerHeldBy.isPlayerDead)
                {
                    __instance.playerHeldBy.externalForces += forces;
                }

                // Update the private fields with the new values
                jetpackPowerField.SetValue(__instance, jetpackPower);
                forcesField.SetValue(__instance, forces);

                // Skip the original method
                return false;
            }

            return true; // Allow other players' jetpacks to function normally
        }

        [HarmonyPatch("ExplodeJetpackServerRpc")]
        [HarmonyPrefix]
        private static bool ExplodeJetpackServerRpcPatch()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("DamagePlayer")]
        [HarmonyPrefix]
        private static bool PrePlayerDamaged(PlayerControllerB __instance, ref int damageNumber, CauseOfDeath causeOfDeath)
        {
            // Prevent the player from taking any damage related to jetpack use or speed
            if (__instance.jetpackControls)
            {
                // Override and prevent damage from high speed or falling
                damageNumber = 0; // Zero out damage to the player
                return false;
            }
            return true;
        }
    }
}
