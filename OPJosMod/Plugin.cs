using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.LagJutsu.Patches;
using OPJosMod.LagJutsu.Utils;
using UnityEngine.InputSystem;

namespace OPJosMod.LagJutsu
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.LagJutsu";
        private const string modName = "LagJutsu";
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

            PlayerControllerBPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(PlayerControllerBPatch));

            EnemyAIPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(EnemyAIPatch));

            ForestGiantAIPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(ForestGiantAIPatch));

            FlowermanAIPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(FlowermanAIPatch));

            CentipedeAIPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(CentipedeAIPatch));

            CrawlerAIPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(CrawlerAIPatch));

            MouthDogAIPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(MouthDogAIPatch));

            HauntedMaskItemPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(HauntedMaskItemPatch));

            MaskedPlayerEnemyPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(MaskedPlayerEnemyPatch));

            PlayerControllerBPatch.SetupVersion(Instance, harmony);
        }

        private void setupConfig()//example config setup
        {
            var configDeathToggleButton = Config.Bind("God Mode Toggle", // The section under which the option is shown
                                        "GodModeToggle",  // The key of the configuration option in the configuration file
                                        Key.K, // The default value
                                        "Button used to toggle God Mode"); // Description of the option to show in the config file

            var configKillSelfButton = Config.Bind("Kill Self Button", 
                                        "KillSelfButton",  
                                        Key.L,
                                        "Button used to off yourself"); 

            ConfigVariables.DeathToggleButton = configDeathToggleButton.Value;
            ConfigVariables.KillSelfButton = configKillSelfButton.Value;
        }
    }
}
