using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GhostMode.Enemy.Patches;
using OPJosMod.GhostMode.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.GhostMode
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.GhostMode";
        private const string modName = "GhostMode";
        private const string modVersion = "2.1.0";

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
            StartOfRoundPatch.SetLogSource(mls);
            EnemyAIPatch.SetLogSource(mls);
            HUDManagerPatch.SetLogSource(mls);
            CentipedeAIPatch.SetLogSource(mls);
            MouthDogAIPatch.SetLogSource(mls);
            ForestGiantAIPatch.SetLogSource(mls);
            SandSpiderAIPatch.SetLogSource(mls);
            NutcrackerEnemyAIPatch.SetLogSource(mls);

            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof (CentipedeAIPatch));
            harmony.PatchAll(typeof(MouthDogAIPatch));
            harmony.PatchAll(typeof(ForestGiantAIPatch));
            harmony.PatchAll(typeof(SandSpiderAIPatch));
            harmony.PatchAll(typeof(NutcrackerEnemyAIPatch));
        }

        private void setupConfig()
        {
            var configWaitTimeBetweenInteractions = Config.Bind("GhostMode interaction delay", // The section under which the option is shown
                                        "GhostModeInteractionDelay",  // The key of the configuration option in the configuration file
                                        45f, // The default value
                                        "How long you must wait between interactions when in ghost mode"); // Description of the option to show in the config file


            var configStartGhostModeButton = Config.Bind("Start Ghost Mode Button",
                                        "StartGhostModeButton",
                                        "P",
                                        "Button to turn into ghost");

            ConfigVariables.waitTimeBetweenInteractions = configWaitTimeBetweenInteractions.Value;
            ConfigVariables.startGhostModeButton = configStartGhostModeButton.Value;
        }
    }
}
