using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.SupahNinja.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace OPJosMod.SupahNinja.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void startPatch(PlayerControllerB __instance)
        {

        }

        [HarmonyPatch("KillPlayerClientRpc")]
        [HarmonyPrefix]
        static bool killPlayerClientRpcPatch(PlayerControllerB __instance)
        {
            return false;
        }

        [HarmonyPatch("Crouch")]
        [HarmonyPrefix]
        static void crouchPatch(PlayerControllerB __instance)
        {
            int playerId = (int)StartOfRound.Instance.localPlayerController.playerClientId;
            bool spawnBody = false;
            Vector3 bodyVelocity = new Vector3(0, 0, 0);
            int causeOfDeath = 0;
            int deathAnimation = 0;

            ReflectionUtils.InvokeMethod(__instance, "KillPlayerServerRpc", new object[] { playerId, spawnBody, bodyVelocity, causeOfDeath, deathAnimation });
        }
    }
}
