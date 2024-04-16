using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.HealthRegen.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.HealthRegen
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.HealthRegen";
        private const string modName = "Health Regen";
        private const string modVersion = "1.1.0";

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

            harmony.PatchAll(typeof(OpJosMod));

            Patches.PlayerControllerBPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(PlayerControllerBPatch));
        }

        private void setupConfig()//example config setup
        {
            var configHealFrequency = Config.Bind("Heal Frequency",
                                        "HealFrequency",
                                        10f,
                                        "How many seconds between each time you heal");

            var configHealthToAdd = Config.Bind("Heal Amount",
                                        "HealAmount",
                                        4,
                                        "How much you heal");

            ConfigVariables.healFrequency = configHealFrequency.Value;
        }
    }
}
