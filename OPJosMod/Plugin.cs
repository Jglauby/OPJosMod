﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.LagJutsu.Patches;
using UnityEngine.InputSystem;

namespace OPJosMod.LagJutsu
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.LagJutsu";
        private const string modName = "LagJutsu";
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

            mls.LogInfo("mod has started");
            setupConfig();

            PlayerControllerBPatch.SetLogSource(mls);
            EnemyAIPatch.SetLogSource(mls);
            ForestGiantAIPatch.SetLogSource(mls);
            FlowermanAIPatch.SetLogSource(mls);
            CentipedeAIPatch.SetLogSource(mls);
            CrawlerAIPatch.SetLogSource(mls);
            MouthDogAIPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()//example config setup
        {
            var configDeathToggleButton = Config.Bind("God Mode Toggle", // The section under which the option is shown
                                        "GodModeToggle",  // The key of the configuration option in the configuration file
                                        Key.K, // The default value
                                        "Button used to toggle God Mode"); // Description of the option to show in the config file

            ConfigVariables.DeathToggleButton = configDeathToggleButton.Value;
        }
    }
}
