using BepInEx;
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
            setupConfig();

            RoundManagerPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()//example config setup
        {
            var configEnemySpawnMultiplier = Config.Bind("Enemy Spawn Multiplier",
                                        "EnemySpawnMultiplier",
                                        7,
                                        "How many more enemies do you want? 5 = 5 times as many enemies.");

            ConfigVariables.enemySpawnMultiplier = configEnemySpawnMultiplier.Value;
        }
    }
}
