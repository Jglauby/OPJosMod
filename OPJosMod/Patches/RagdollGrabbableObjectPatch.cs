using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.ReviveCompany.CustomRpc;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OPJosMod.ReviveCompany.Patches
{
    [HarmonyPatch(typeof(RagdollGrabbableObject))]
    internal class RagdollGrabbableObjectPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void patchStart(RagdollGrabbableObject __instance)
        {
            if (!GlobalVariables.ModActivated)
                return;

            //mls.LogMessage($"og weight of body: {__instance.itemProperties.weight}");
            __instance.itemProperties.weight = ConfigVariables.DeadPlayerWeight;
        }
    }
}
