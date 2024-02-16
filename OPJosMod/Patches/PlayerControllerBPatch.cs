using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using System;
using System.Xml.Linq;

namespace OPJosMod.ReviveTeam.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("Crouch")]
        [HarmonyPrefix]
        static void crouchPatch(PlayerControllerB __instance)
        {
            if (__instance.IsOwner && (!__instance.IsServer || __instance.isHostPlayerObject))
            {
                mls.LogMessage("you crouched!");
                ReviveDeadPlayer(__instance);
            }
        }

        private static void ReviveDeadPlayer(PlayerControllerB __instance)
        {
            try
            {
                LethalServerMessage<int> customServerMessage = new LethalServerMessage<int>(identifier: "customIdentifier");
                customServerMessage.SendAllClients(3);
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }    
        }
    }
}
