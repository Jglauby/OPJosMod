using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.HideNSeek.Config;
using OPJosMod.HideNSeek.CustomRpc;
using OPJosMode.HideNSeek.Patches;

namespace OPJosMod.HideNSeek
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.HideNSeek";
        private const string modName = "HideNSeek";
        private const string modVersion = "1.2.0";

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

            RpcMessageHandler.SetLogSource(mls);
            HUDManagerPatchForRPC.SetLogSource(mls);
            CompleteRecievedTasks.SetLogSource(mls);
            PlayerControllerBPatch.SetLogSource(mls);
            StartOfRoundPatch.SetLogSource(mls);
            StartMatchLeverPatch.SetLogSource(mls);
            RoundManagerPatch.SetLogSource(mls);
            HUDManagerPatch.SetLogSource(mls);  
            EntranceTeleportPatch.SetLogSource(mls);
            ShovelPatch.SetLogSource(mls);
            TerminalPatch.SetLogSource(mls);
            GeneralUtil.SetLogSource(mls);
            FlashlightItemPatch.SetLogSource(mls);
            TurretPatch.SetLogSource(mls);
            TimeOfDayPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configSeekerDelay = Config.Bind("Seeker Delay", // The section under which the option is shown
                                        "SeekerDelay",  // The key of the configuration option in the configuration file
                                        50f, // The default value
                                        "How long until the seeker can begin seeking"); // Description of the option to show in the config file
            
            var configSeekerAbilityCooldown = Config.Bind("Seeker Ability CD",
                                        "SeekerAbilityCD",
                                        25f,
                                        "Cooldown for seekers ability");

            var configFlashlightPower = Config.Bind("Small Flashlight Power",
                                        "SmallFlashlightPower",
                                        2,
                                        "Fraction of brightness, ex) 5 = 1/5th power");

            var configFlashlightWeight = Config.Bind("Pro Flashlight Weight",
                                        "ProFlashlightWeight",
                                        1.25f,
                                        "Weight Increase multiplier, higher number greater the weight.");

            var configFlashlightBatteryUsage = Config.Bind("Pro Flashlight Battery Usage",
                                        "ProFlashlightBatteryUsage",
                                        0.5f,
                                        "Influences how fast the battery depletes on the pro flashlight, 1 = games default speed");

            var configSeekerSprintMultiplier = Config.Bind("Seeking Sprint Multiplier",
                                        "SeekingSprintMultiplier",
                                        1.01f,
                                        "How fast you accelerate as a seeker");

            var configSeekerSprintTopSpeed = Config.Bind("Seeking Sprint Top Speed",
                                        "SeekingSprintTopSpeed",
                                        3f,
                                        "The fastest the seeker can move");

            var configDaySpeedMultiplier = Config.Bind("Day Speed Multiplier",
                                        "DaySpeedMultiplier",
                                        2.5f,
                                        "Higher the number the faster days go, day speed auto adjusts with players");

            var configSpawnScrap = Config.Bind("Spawn Scrap",
                                        "SpawnScrap",
                                        false,
                                        "should items scrap?");

            var configJumpForce = Config.Bind("Jump Force",
                                        "JumpForce",
                                        15f,
                                        "How strong your jump is, game default is 5");

            var configSeekerStartingItem1 = Config.Bind("Seeker Item 1",
                                        "SeekerItem1",
                                        BuyableItems.Shovel,
                                        "Spawned Item for seeker");

            var configSeekerStartingItem2 = Config.Bind("Seeker Item 2",
                                        "SeekerItem2",
                                        BuyableItems.Flashlight,
                                        "Spawned Item for seeker");

            var configSeekerStartingItem3 = Config.Bind("Seeker Item 3",
                                        "SeekerItem3",
                                        BuyableItems.None,
                                        "Spawned Item for seeker");

            var configSeekerStartingItem4 = Config.Bind("Seeker Item 4",
                                        "SeekerItem4",
                                        BuyableItems.None,
                                        "Spawned Item for seeker");

            var configHiderItem = Config.Bind("Hider Item",
                                        "HidingItem",
                                        BuyableItems.StunGrenade,
                                        "Give Hiders some item to help");

            ConfigVariables.seekerDelay = configSeekerDelay.Value;
            ConfigVariables.seekerAbilityCD = configSeekerAbilityCooldown.Value;
            ConfigVariables.smallFlashlightPower = configFlashlightPower.Value;
            ConfigVariables.proFlashlightWeightMultiplier = configFlashlightWeight.Value;
            ConfigVariables.proFlashlightBatteryUsageMultiplier = configFlashlightBatteryUsage.Value;
            ConfigVariables.seekerSprintMultiplier = configSeekerSprintMultiplier.Value;
            ConfigVariables.seekerMaxSprintSpeed = configSeekerSprintTopSpeed.Value;
            ConfigVariables.daySpeedMultiplier = configDaySpeedMultiplier.Value;
            ConfigVariables.shouldSpawnScrap = configSpawnScrap.Value;
            ConfigVariables.jumpForce = configJumpForce.Value;
            ConfigVariables.seekerItem1 = configSeekerStartingItem1.Value;
            ConfigVariables.seekerItem2 = configSeekerStartingItem2.Value;
            ConfigVariables.seekerItem3 = configSeekerStartingItem3.Value;
            ConfigVariables.seekerItem4 = configSeekerStartingItem4.Value;
            ConfigVariables.hiderItem = configHiderItem.Value;
        }
    }
}
