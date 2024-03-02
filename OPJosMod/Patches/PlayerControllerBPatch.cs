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
        private static Coroutine lockPlayerCoroutine;

        public static void SetupHider()
        {
            mls.LogMessage("setup player as a hider");
            PlayerControllerB localPlayerController = StartOfRound.Instance.localPlayerController;
            isSeeker = false;
            isHider = true;

            //do nothing for a couple seconds
            lockPlayerCoroutine = localPlayerController.StartCoroutine(lockPlayer(localPlayerController, 2f));

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

            lockPlayerCoroutine = localPlayerController.StartCoroutine(lockPlayer(localPlayerController, 15f));

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

            yield return new WaitForSeconds(0.1f);

            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(location);
        }

        private static IEnumerator lockPlayer(PlayerControllerB player, float lockTime)
        {
            if (lockPlayerCoroutine != null)
            {
                player.StopCoroutine(lockPlayerCoroutine);
            }

            yield return new WaitForSeconds(1f);
            mls.LogMessage("player locked in place");
            player.playerCollider.enabled = false;

            //add some sort of UI to indicate you are waiting and for how long
            //will need to loop with smaller delays to update the UI every second

            yield return new WaitForSeconds(lockTime);
            mls.LogMessage("player unlocked!");
            player.playerCollider.enabled = true;
        }
    }
}
