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
                                        3,
                                        "Fraction of brightness, ex) 5 = 1/5th power");

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

            ConfigVariables.seekerDelay = configSeekerDelay.Value;
            ConfigVariables.seekerAbilityCD = configSeekerAbilityCooldown.Value;
            ConfigVariables.smallFlashlightPower = configFlashlightPower.Value;
            ConfigVariables.seekerSprintMultiplier = configSeekerSprintMultiplier.Value;
            ConfigVariables.seekerMaxSprintSpeed = configSeekerSprintTopSpeed.Value;
            ConfigVariables.daySpeedMultiplier = configDaySpeedMultiplier.Value;
            ConfigVariables.shouldSpawnScrap = configSpawnScrap.Value;
        }
    }
}
