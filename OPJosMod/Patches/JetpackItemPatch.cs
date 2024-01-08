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
        static void Update(JetpackItem __instance)
        {
            //whole goal is to recreate the update funciton but without the killing part
            if (__instance != null)
            {
                FieldInfo rayHitField = typeof(JetpackItem).GetField("rayHit", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo forcesField = typeof(JetpackItem).GetField("forces", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo jetpackPowerField = typeof(JetpackItem).GetField("jetpackPower", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo jetpackActivatedField = typeof(JetpackItem).GetField("jetpackActivated", BindingFlags.NonPublic | BindingFlags.Instance);

                if (rayHitField != null && forcesField != null && jetpackPowerField != null && jetpackActivatedField != null)
                {
                    RaycastHit rayHit = (RaycastHit)rayHitField.GetValue(__instance);
                    Vector3 forces = (Vector3)forcesField.GetValue(__instance);
                    float jetpackPower = (float)jetpackPowerField.GetValue(__instance);
                    bool jetpackActivated = (bool)jetpackActivatedField.GetValue(__instance);

                    //make player not take fall damage
                    __instance.playerHeldBy.takingFallDamage = false;

                    //----------update function---------------
                    //base.Update();                    
                    if (jetpackActivated)
                    {
                        jetpackPower = Mathf.Clamp(jetpackPower + Time.deltaTime * 10f, 0f, 500f);
                    }
                    else
                    {
                        jetpackPower = Mathf.Clamp(jetpackPower - Time.deltaTime * 75f, 0f, 1000f);
                        if (__instance.playerHeldBy.thisController.isGrounded)
                        {
                            jetpackPower = 0f;
                        }
                    }
                    forces = Vector3.Lerp(forces, Vector3.ClampMagnitude(__instance.playerHeldBy.transform.up * jetpackPower, 400f), Time.deltaTime);
                    if (!__instance.playerHeldBy.jetpackControls)
                    {
                        forces = Vector3.zero;
                    }

                    if (!__instance.playerHeldBy.isPlayerDead &&
                        Physics.Raycast(__instance.playerHeldBy.transform.position, forces, out rayHit, 25f, StartOfRound.Instance.allPlayersCollideWithMask) &&
                        forces.magnitude - rayHit.distance > 50f &&
                        rayHit.distance < 4f)
                    {
                        //if (mls != null) { mls.LogInfo("SaferJetpack(success): should kill but won't"); }
                        throw new Exception("SaferJetpack: Cancelling original Update method");
                    }
                    //-----update funciton-------
                }
                else
                {
                    if (mls != null) { mls.LogInfo("SaferJetpack(error): couldn't find a private field"); }
                }
            }
            else
            {
                if (mls != null) { mls.LogInfo("SaferJetpack(error): __instance == null"); }
            }
        }
    }
}
