﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.LagJutsu.Utils;
using UnityEngine;

namespace OPJosMod.LagJutsu.Patches
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemyPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("OnCollideWithPlayer")]
        [HarmonyPrefix]
        static bool onCollideWithPlayerPatch(ref Collider other)
        {
            if (PlayerControllerBPatch.godMode)
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                if (StartOfRound.Instance.localPlayerController.playerClientId == component.playerClientId)
                {
                    //mls.LogMessage("masked enemy collide with player patch hit");
                    PlayerControllerBPatch.teleportPlayerBack();
                    return false;
                }
            }

            return true;
        }
    }
}
