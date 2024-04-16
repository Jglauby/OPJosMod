﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.SaferJeptack
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.SaferJetpack";
        private const string modName = "SaferJetpack";
        private const string modVersion = "1.3.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        private static OpJosMod Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("SaferJetpack mod has started");

            harmony.PatchAll(typeof(OpJosMod));
            Patches.JetpackItemPatch.SetLogSource(mls);

            harmony.PatchAll(typeof(Patches.JetpackItemPatch));
        }
    }
}
