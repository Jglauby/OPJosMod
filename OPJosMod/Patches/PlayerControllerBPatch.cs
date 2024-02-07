using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.TheFlash.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void patchUpdate(PlayerControllerB __instance)
        {
            if (__instance.isSprinting)
            {
                FieldInfo sprintMultiplierField = typeof(PlayerControllerB).GetField("sprintMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
                if (sprintMultiplierField != null)
                {
                    var currentValue = sprintMultiplierField.GetValue(__instance);
                    if (currentValue is float)
                    {
                        var newForce = (float)currentValue * 1.05f;
                        sprintMultiplierField.SetValue(__instance, newForce);
                    }
                    else
                    {
                        mls.LogError("current spritnMultiplier isn't a float?");
                    }
                }
                else
                {
                    mls.LogError("private field not found");
                }
            }
        }
    }
}
