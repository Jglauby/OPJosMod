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
    [HarmonyPatch(typeof(ShipTeleporter))]
    internal class ShipTeleporterPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("beamUpPlayer")]
        [HarmonyPrefix]
        private static void patchbeamUpPlayer()
        {
            if (!GlobalVariables.ModActivated)
                return;

            var playerBeamedUp = StartOfRound.Instance.mapScreen.targetedPlayer;
            if (playerBeamedUp == null || playerBeamedUp.redirectToEnemy != null || playerBeamedUp.deadBody == null)            
                return;

            if (playerBeamedUp.isPlayerDead)
                GlobalVariables.DeadBodiesTeleported.Add((int)playerBeamedUp.playerClientId);
        }
    }
}
