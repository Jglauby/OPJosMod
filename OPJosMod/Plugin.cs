using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.OPClientSide.Patches;
using UnityEngine.InputSystem;

namespace OPJosMod.OPClientSide
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.OPClientSide";
        private const string modName = "OPClientSide";
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
            StartOfRoundPatch.SetLogSource(mls);
            EnemyAIPatch.SetLogSource(mls);
            HUDManagerPatch.SetLogSource(mls);
            ShovelPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            //var configSeeOtherGhosts = Config.Bind("See Other Ghosts",
            //                            "SeeOtherGhosts",
            //                            false,
            //                            "Everyone needs this mod installed and have this toggled on for this to work, but it will allow ghosts to see eachother run around!");


            var configStartGhostModeButton = Config.Bind("Start Ghost Mode Button",
                                        "StartGhostModeButton",
                                        Key.P,
                                        "Button to turn into ghost");

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

            var configGodModeOffButton = Config.Bind("Turn Off God Mode Button",
                            "TurnOffGodModeButton",
                            Key.K,
                            "Button to allow yourself to die");

            var configGodModeButton = Config.Bind("Turn on God Mode",
                            "GodModeButton",
                            Key.L,
                            "Button to turn on God Mode");

            var configKYSButton = Config.Bind("Button to KYS",
                            "KYSButton",
                            Key.Semicolon,
                            "Button that insta offs yourself, god mode must be off");

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

            ConfigVariables.noClipForwardButton = configNoClipFlyForwardButton.Value;
            ConfigVariables.noClipBackwardButton = configNoClipFlyBackwardButton.Value;
            ConfigVariables.noClipLeftButton = configNoClipFlyLeftButton.Value;
            ConfigVariables.noClipRightButton = configNoClipFlyRightButton.Value;
            ConfigVariables.noClipUpButton = configNoClipFlyUpButton.Value;
            ConfigVariables.noClipDownButton = configNoClipFlyDownButton.Value;

            ConfigVariables.godModeOffButton = configGodModeOffButton.Value;
            ConfigVariables.godModeButton = configGodModeButton.Value;
            ConfigVariables.kysButton = configKYSButton.Value;

            Config.Save();
        }
    }
}
