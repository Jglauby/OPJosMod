﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.OneHitShovel.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("AddPlayerChatMessageServerRpc")]
        [HarmonyPrefix]
        private static void addPlayerChatMessageServerRpcPatch(ref string chatMessage, ref int playerId)
        {
            mls.LogMessage($"chat message:{chatMessage} playerID:{playerId}");
        }
    }
}