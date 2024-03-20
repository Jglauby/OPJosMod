using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.SupahNinja.Enemy.Patches;
using OPJosMod.SupahNinja.Patches;

namespace OPJosMod.SupahNinja
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.SupahNinja";
        private const string modName = "SupahNinja";
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
            EnemyAIPatch.SetLogSource(mls);
            CentipedeAIPatch.SetLogSource(mls);
            MouthDogAIPatch.SetLogSource(mls);
            ForestGiantAIPatch.SetLogSource(mls);
            SandSpiderAIPatch.SetLogSource(mls);
            NutcrackerEnemyAIPatch.SetLogSource(mls);
            LandminePatch.SetLogSource(mls);
            FlowermanAIPatch.SetLogSource(mls);
            CrawlerAIPatch.SetLogSource(mls);
            TurretPatch.SetLogSource(mls);
            MaskedPlayerEnemyPatch.SetLogSource(mls);
            JesterAIPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configAlwaysSneak = Config.Bind("Always Sneaking",
                                        "AlwaysSneaking",
                                        false,
                                        "do  you want to be hiding from enemy detection all the time or only when crouched?");

            ConfigVariables.alwaysSneak = configAlwaysSneak.Value;
            Config.Save();
        }
    }
}
