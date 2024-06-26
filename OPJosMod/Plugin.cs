﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.MODNAMEHERE.CustomRpc;
using OPJosMod.Patches;

namespace OPJosMod.ModNameHere
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.ModNameHere";
        private const string modName = "ModNameHere";
        private const string modVersion = "1.0.0";

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
            setupConfig();

            PatchesForRPC.SetLogSource(mls);
            RpcMessageHandler.SetLogSource(mls);
            CompleteRecievedTasks.SetLogSource(mls);

            PlayerControllerBPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()//example config setup
        {
            var configSprintMultiplier = Config.Bind("Sprint Multiplier", // The section under which the option is shown
                                        "SprintMultiplier",  // The key of the configuration option in the configuration file
                                        1.04f, // The default value
                                        "How fast your speed rams up when sprinting"); // Description of the option to show in the config file

            var configFlashTimeButton = Config.Bind("Flash Time Button",
                                        "FlashTimeButton",
                                        "R",
                                        "Button used to toggle flash time");

            ConfigVariables.defaultSprintMultiplier = configSprintMultiplier.Value;
            ConfigVariables.flashTimeButton = configFlashTimeButton.Value;
        }
    }
}
