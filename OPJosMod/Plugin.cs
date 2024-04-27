﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.MoreEnemies.Patches;

namespace OPJosMod.MoreEnemies
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.MoreEnemies";
        private const string modName = "MoreEnemies";
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
            setupConfig();

            RoundManagerPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configEnemySpawnMultiplier = Config.Bind("Enemy Spawn Multiplier",
                                        "EnemySpawnMultiplier",
                                        2.5f,
                                        "How many more enemies do you want?");

            ConfigVariables.enemySpawnMultiplier = configEnemySpawnMultiplier.Value;
        }
    }
}
