using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.BreadCrumbs.Utils;
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

namespace OPJosMod.BreadCrumbs.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntranceTeleportPatch : MonoBehaviour
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("TeleportPlayer")]
        [HarmonyPostfix]
        static void TeleportPlayerPatch()
        {
            PlayerControllerBPatch.InitializeNaveMeshForPlayer();
        }      
    }
}
