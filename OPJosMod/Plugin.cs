using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GhostMode.Enemy.Patches;
using OPJosMod.GhostMode.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OPJosMod.GhostMode
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.GhostMode";
        private const string modName = "GhostMode";
        private const string modVersion = "2.3.0";

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
            StartMatchLeverPatch.SetLogSource(mls);

            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof (CentipedeAIPatch));
            harmony.PatchAll(typeof(MouthDogAIPatch));
            harmony.PatchAll(typeof(ForestGiantAIPatch));
            harmony.PatchAll(typeof(SandSpiderAIPatch));
            harmony.PatchAll(typeof(NutcrackerEnemyAIPatch));
            harmony.PatchAll(typeof(StartMatchLeverPatch));
        }

        private void setupConfig()
        {
            var configOPness = Config.Bind("OP-Ness",
                                        "OPness",
                                        "balanced",
                                        "(limited, balanced, unrestricted) The three modes of ghost mode. (limited -> almost no interactions allowed.) (balanced -> delays on lots of interactions. completly restricted from a few.) (unrestricted -> no restrictions on what you can interact with at all)");

            var configWaitTimeBetweenInteractions = Config.Bind("GhostMode interaction delay", // The section under which the option is shown
                                        "GhostModeInteractionDelay",  // The key of the configuration option in the configuration file
                                        45f, // The default value
                                        "How long you must wait between interactions when in ghost mode. Set to -1 to remove the ability to interact at all"); // Description of the option to show in the config file

            var configStartGhostModeButton = Config.Bind("Start Ghost Mode Button",
                                        "StartGhostModeButton",
                                        "P",
                                        "Button to turn into ghost");

            var configTeleportBodyButton = Config.Bind("Teleport to Dead Body Button",
                                        "TeleportToDeadBodyButton",
                                        "Backspace",
                                        "Button to teleport to your dead body");

            var configToggleBrightModeButton = Config.Bind("Toggle Bright Mode Button",
                                        "ToggleBrightModeButton",
                                        "B",
                                        "Button to toggle on bright mode");

            var configTeleportFrontDoorButton = Config.Bind("Teleport to Front Door Button",
                                        "TeleportToFrontDoorButton",
                                        "UpArrow",
                                        "Button to teleport to the front door");

            var configTeleportShipButton = Config.Bind("Teleport to Ship Button",
                                        "TeleportToShipButton",
                                        "DownArrow",
                                        "Button to teleport you to the ship");

            var configSwitchToSpectateButton = Config.Bind("Switch to Spectate Mode Button",
                                        "SwitchToSpectateModeButton",
                                        "O",
                                        "Button to switch back to specate mode");

            var configToggleNoClipButton = Config.Bind("Toggle NoClip Mode Button",
                                        "ToggleNoClipModeButton",
                                        "Z",
                                        "Button to enter/leave no clip mode");

            var configNoClipFlySpeed = Config.Bind("NoClip Flight Speed",
                                        "NoClipFlightSpeed",
                                        0.27f,
                                        "How fast you move while in no clip");
           
            ConfigVariables.waitTimeBetweenInteractions = configWaitTimeBetweenInteractions.Value;

            ConfigVariables.startGhostModeButton = ValidateAndAssignButton(configStartGhostModeButton, "P");
            ConfigVariables.teleportBodyButton = ValidateAndAssignButton(configTeleportBodyButton, "Backspace");
            ConfigVariables.toggleBrightModeButton = ValidateAndAssignButton(configToggleBrightModeButton, "B");
            ConfigVariables.teleportFrontDoorButton = ValidateAndAssignButton(configTeleportFrontDoorButton, "UpArrow");
            ConfigVariables.switchToSpectateButton = ValidateAndAssignButton(configSwitchToSpectateButton, "O");
            ConfigVariables.toggleNoClipButton = ValidateAndAssignButton(configToggleNoClipButton, "Z");
            ConfigVariables.teleportShipButton = ValidateAndAssignButton(configTeleportShipButton, "DownArrow");

            ConfigVariables.noClipSpeed = configNoClipFlySpeed.Value;
            ConfigVariables.OPness = ValidateAndAssignOPness(configOPness);

            Config.Save();
        }

        private string ValidateAndAssignButton(ConfigEntry<string> configEntry, string defaultButton)
        {
            if (Enum.IsDefined(typeof(Key), configEntry.Value))
            {
                return configEntry.Value;
            }
            else
            {
                mls.LogError($"{configEntry.Value} is not a valid mapped button!");

                configEntry.Value = defaultButton;
                return defaultButton;
            }
        }

        private string ValidateAndAssignOPness(ConfigEntry<string> configEntry)
        {
            string[] modes = new string[] { "limited", "balanced", "unrestricted" };
            
            if (modes.Contains(configEntry.Value?.ToLower()))
            {
                return configEntry.Value;
            }
            else
            {
                configEntry.Value = "balanced";
                return "balanced";
            }
        }
    }
}
