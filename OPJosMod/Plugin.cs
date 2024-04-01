using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.TheFlash.Patches;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OPJosMod.TheFlash
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.TheFlash";
        private const string modName = "TheFlash";
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
            mls.LogInfo("mod has started");

            setupConfig();
            //Config.SettingChanged += (obj, args) =>
            //{
            //    mls.LogMessage("config chagned!!!!");
            //    setupConfig();
            //    patch();
            //};

            patch();
        }

        private void patch()
        {
            Patches.PlayerControllerBPatch.SetLogSource(mls);
            EntranceTeleportPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configSprintMultiplier = Config.Bind("Sprint Multiplier", // The section under which the option is shown
                                        "SprintMultiplier",  // The key of the configuration option in the configuration file
                                        1.04f, // The default value
                                        "How fast your speed rams up when sprinting"); // Description of the option to show in the config file

            var configMaxSprintSpeed = Config.Bind("Max Sprint Speed", 
                                        "MaxSprintSpeed",  
                                        15f, 
                                        "How fast you can run with flash time off");

            var configWalkMultiplier = Config.Bind("Walk Multiplier",
                                        "WalkMultiplier",
                                        1.05f,
                                        "How fast you accelerate to top speed while walking");

            var configMaxWalkSpeed = Config.Bind("Max Walk Speed",
                                        "MaxWalkSpeed",
                                        8f,
                                        "How fast you walk");

            var configFlashTimeButton = Config.Bind("Flash Time Button",
                                        "FlashTimeButton",
                                        Key.R,
                                        "Button used to toggle flash time");

            ConfigVariables.defaultSprintMultiplier = configSprintMultiplier.Value;
            ConfigVariables.defaultMaxSprintSpeed = configMaxSprintSpeed.Value;
            ConfigVariables.walkMultiplier = configWalkMultiplier.Value;
            ConfigVariables.maxWalkSpeed = configMaxWalkSpeed.Value;
            ConfigVariables.flashTimeButton = configFlashTimeButton.Value;
        }
    }
}
