using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GhostMode.CustomRpc;
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
        private const string modVersion = "2.6.1";

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

            CompleteRecievedTasks.SetLogSource(mls);
            PatchesForRPC.SetLogSource(mls);
            RpcMessageHandler.SetLogSource(mls);

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
            LandminePatch.SetLogSource(mls);
            FlowermanAIPatch.SetLogSource(mls);
            CrawlerAIPatch.SetLogSource(mls);
            TurretPatch.SetLogSource(mls);
            MaskedPlayerEnemyPatch.SetLogSource(mls);
            JesterAIPatch.SetLogSource(mls);
            ShovelPatch.SetLogSource(mls);
            RadMechAIPatch.SetLogSource(mls);
            ButlerEnemyAIPatch.SetLogSource(mls);
            SpikeRoofTrapPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configOPness = Config.Bind("OP-Ness",
                                        "OPness",
                                        OPnessModes.Balanced,
                                        "(limited, balanced, unrestricted) The three modes of ghost mode. (limited -> almost no interactions allowed.) (balanced -> delays on lots of interactions. completly restricted from a few.) (unrestricted -> no restrictions on what you can interact with at all)");

            var configWaitTimeBetweenInteractions = Config.Bind("GhostMode interaction delay", // The section under which the option is shown
                                        "GhostModeInteractionDelay",  // The key of the configuration option in the configuration file
                                        45f, // The default value
                                        "How long you must wait between interactions when in ghost mode. Set to -1 to remove the ability to interact at all"); // Description of the option to show in the config file

            var configStartGhostModeButton = Config.Bind("Start Ghost Mode Button",
                                        "StartGhostModeButton",
                                        Key.P,
                                        "Button to turn into ghost");

            var configCanGrabScrap = Config.Bind("Can Grab Scrap",
                                        "CanGrabScrap",
                                        true,
                                        "this setting only has an effect if you are in balanced mode");

            var configTeleportBodyButton = Config.Bind("Teleport to Dead Body Button",
                                        "TeleportToDeadBodyButton",
                                        Key.Backspace,
                                        "Button to teleport to your dead body");

            var configToggleBrightModeButton = Config.Bind("Toggle Bright Mode Button",
                                        "ToggleBrightModeButton",
                                        Key.B,
                                        "Button to toggle on bright mode");

            var configTeleportFrontDoorButton = Config.Bind("Teleport to Front Door Button",
                                        "TeleportToFrontDoorButton",
                                        Key.UpArrow,
                                        "Button to teleport to the front door");

            var configTeleportShipButton = Config.Bind("Teleport to Ship Button",
                                        "TeleportToShipButton",
                                        Key.DownArrow,
                                        "Button to teleport you to the ship");

            var configSwitchToSpectateButton = Config.Bind("Switch to Spectate Mode Button",
                                        "SwitchToSpectateModeButton",
                                        Key.O,
                                        "Button to switch back to specate mode");

            var configEnemyDetection = Config.Bind("Enemies Detect Ghost",
                                        "EnemiesDetectGhost",
                                        false,
                                        "Enemies are able to detect you as a ghost, true or false");

            var configTeleportPlayerToPlayer1 = Config.Bind("Teleport to Player 1",
                                        "TeleportToPlayerForward",
                                        Key.RightArrow,
                                        "Button to other players forward in the list of players");

            var configTeleportPlayerToPlayer2 = Config.Bind("Teleport to Player 2",
                                        "TeleportToPlayerBackward",
                                        Key.LeftArrow,
                                        "Button to teleport you to other players backwards in the list of players");

            var configToggleNoClipButton = Config.Bind("Toggle NoClip Mode Button",
                            "ToggleNoClipModeButton",
                            Key.Z,
                            "Button to enter/leave no clip mode");

            var configNoClipFlySpeed = Config.Bind("NoClip Flight Speed",
                                        "NoClipFlightSpeed",
                                        0.27f,
                                        "How fast you move while in no clip");

            var configNoClipFlyForwardButton = Config.Bind("NoClip Forward Button",
                            "NoClipForwardButton",
                            Key.W,
                            "Button to move forward in no clip mode");

            var configNoClipFlyBackwardButton = Config.Bind("NoClip Backward Button",
                            "NoClipBackwardButton",
                            Key.S,
                            "Button to move backwards in no clip mode");

            var configNoClipFlyLeftButton = Config.Bind("NoClip Left Button",
                            "NoClipLeftButton",
                            Key.A,
                            "Button to move left in no clip mode");

            var configNoClipFlyRightButton = Config.Bind("NoClip Right Button",
                            "NoClipRightButton",
                            Key.D,
                            "Button to move right in no clip mode");

            var configNoClipFlyUpButton = Config.Bind("NoClip Up Button",
                            "NoClipUpButton",
                            Key.Space,
                            "Button to move up in no clip mode");

            var configNoClipFlyDownButton = Config.Bind("NoClip Down Button",
                            "NoClipDownButton",
                            Key.LeftShift,
                            "Button to move down in no clip mode");

            ConfigVariables.waitTimeBetweenInteractions = configWaitTimeBetweenInteractions.Value;
            ConfigVariables.canPickupScrap = configCanGrabScrap.Value;

            ConfigVariables.startGhostModeButton = configStartGhostModeButton.Value;
            ConfigVariables.teleportBodyButton = configTeleportBodyButton.Value;
            ConfigVariables.toggleBrightModeButton = configToggleBrightModeButton.Value;
            ConfigVariables.teleportFrontDoorButton = configTeleportFrontDoorButton.Value;
            ConfigVariables.switchToSpectateButton = configSwitchToSpectateButton.Value;
            ConfigVariables.toggleNoClipButton = configToggleNoClipButton.Value;
            ConfigVariables.teleportShipButton = configTeleportShipButton.Value;
            ConfigVariables.teleportToPlayerForwardButton = configTeleportPlayerToPlayer1.Value;
            ConfigVariables.teleportToPlayerBackwardButton = configTeleportPlayerToPlayer2.Value;

            ConfigVariables.noClipSpeed = configNoClipFlySpeed.Value;
            ConfigVariables.OPness = configOPness.Value;
            ConfigVariables.enemiesDetectYou = configEnemyDetection.Value;

            ConfigVariables.noClipForwardButton = configNoClipFlyForwardButton.Value;
            ConfigVariables.noClipBackwardButton = configNoClipFlyBackwardButton.Value;
            ConfigVariables.noClipLeftButton = configNoClipFlyLeftButton.Value;
            ConfigVariables.noClipRightButton = configNoClipFlyRightButton.Value;
            ConfigVariables.noClipUpButton = configNoClipFlyUpButton.Value;
            ConfigVariables.noClipDownButton = configNoClipFlyDownButton.Value;

            Config.Save();
        }
    }
}
