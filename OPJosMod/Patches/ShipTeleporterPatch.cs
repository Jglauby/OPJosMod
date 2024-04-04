using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.TheFlash.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

namespace OPJosMod.TheFlash.Patches
{
    [HarmonyPatch(typeof(ShipTeleporter))]
    internal class ShipTeleporterPatch : MonoBehaviour
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("beamUpPlayer")]
        [HarmonyPrefix]
        static void beamUpPlayerPatch()
        {
            PlayerControllerB playerToBeamUp = StartOfRound.Instance.mapScreen.targetedPlayer;
            if (playerToBeamUp != null && playerToBeamUp.playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                PlayerControllerBPatch.RemoveMeshForPlayer();
            }
        }

        [HarmonyPatch("beamOutPlayer")]
        [HarmonyPostfix]
        static void beamOutPlayerPatch()
        {
            var player = StartOfRound.Instance.localPlayerController;

            if (player.isInsideFactory)
                PlayerControllerBPatch.InitializeNaveMeshForPlayer();
        }
    }
}
