using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

namespace OPJosMod.GhostMode.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool onCollideWithPlayerPatch(EnemyAI __instance)
        {
            if (PlayerControllerBPatch.isGhostMode && __instance.GetClosestPlayer().playerClientId == StartOfRound.Instance.localPlayerController.playerClientId)
            {
                mls.LogMessage("enemy collide with player patch hit");
                return false;
            }

            return true;
        }

        [HarmonyPatch("PlayerIsTargetable")]
        [HarmonyPrefix]
        static void playerIsTargetablePatch(ref bool cannotBeInShip, ref PlayerControllerB playerScript)
        {
            //a way to make current player always return not tragetable in the enemy ai
            if (PlayerControllerBPatch.isGhostMode && playerScript.IsOwner)
            {
                //mls.LogMessage("set local player to not targetable");
                playerScript.isInHangarShipRoom = true;
                cannotBeInShip = true;
            }
        }
    }
}
