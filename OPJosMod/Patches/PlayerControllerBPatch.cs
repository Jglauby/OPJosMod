using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMode.HideNSeek.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static bool isSeeker = false;
        public static bool isHider = false;

        private static Coroutine teleportCoroutine;

        public static void SetupHider()
        {
            mls.LogMessage("setup player as a hider");
            PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
            isSeeker = false;
            isHider = true;

            //set suit to green?

            //teleport player inside
            teleportCoroutine = localPlayerController.StartCoroutine(customTeleportPlayer(localPlayerController, RoundManager.FindMainEntrancePosition()));
            localPlayerController.isInsideFactory = true;
        }

        public static void SetupSeeker()
        {
            mls.LogMessage("setup player as seeker");
            PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
            isSeeker = true;
            isHider = false;

            //increase speed slightly
            //give gun
            //remove scan option
            //force enemies to whistle, on cool down
        }

        private static IEnumerator customTeleportPlayer(PlayerControllerB player, Vector3 location)
        {
            if (teleportCoroutine != null)
            {
                player.StopCoroutine(teleportCoroutine);
            }

            yield return new WaitForSeconds(5f);

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(location);
        }
    }
}
