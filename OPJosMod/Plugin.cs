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
        private const string modVersion = "1.3.1";

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
            StartMatchLeverPatch.SetLogSource(mls);
            GameNetworkManagerPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configSpawnInsideEnemies = Config.Bind("Spawn Inside Enemies",
                                       "SpawnInsideEnemeies",
                                       true,
                                       "Have indoor enemies or not");

            var configEnemyInsideSpawnMultiplier = Config.Bind("Inside Spawn Multiplier",
                                        "InsideSpawnMultiplier",
                                        1.5f,
                                        "How many more enemies do you want inside?");

            var configSpawnOutsideEnemies = Config.Bind("Spawn Outside Enemies",
                                       "SpawnOutsideEnemeies",
                                       true,
                                       "Have outdoor enemies or not");

            var configEnemyOutsideSpawnMultiplier = Config.Bind("Outside Spawn Multiplier",
                                        "OutsideSpawnMultiplier",
                                        1.5f,
                                        "How many more enemies do you want outside?");

            ConfigVariables.spawnEnemiesInside = configSpawnInsideEnemies.Value;
            ConfigVariables.enemyInsideSpawnMultiplier = configEnemyInsideSpawnMultiplier.Value;

            ConfigVariables.spawnEnemiesOutside = configSpawnOutsideEnemies.Value;
            ConfigVariables.enemyOutsideSpawnMultiplier = configEnemyOutsideSpawnMultiplier.Value;
        }
    }
}
