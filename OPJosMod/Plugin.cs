using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.MODNAMEHERE.CustomRpc;
using OPJosMod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod//.ModNameHere
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.ModNameHere";
        private const string modName = "ModNameHere";
        private const string modVersion = "1.0.0.0"; //dont forget to update this lol

        private readonly Harmony harmony = new Harmony(modGUID);
        private static OpJosMod Instance;

        internal ManualLogSource mls;
        //update namespaces, and the strings for names
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("mod has started");
            setupConfig();

            HUDManagerPatchForRPC.SetLogSource(mls);
            RpcMessageHandler.SetLogSource(mls);
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
