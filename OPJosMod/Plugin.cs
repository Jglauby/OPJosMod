using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.OneHitShovel.CustomRpc;
using OPJosMod.OneHitShovel.Patches;

namespace OPJosMod.OneHitShovel
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.OneHitShovel";
        private const string modName = "OneHitShovel";
        private const string modVersion = "1.5.1"; 

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
            PatchesForRPC.SetLogSource(mls);
            RpcMessageHandler.SetLogSource(mls);
            CompleteRecievedTasks.SetLogSource(mls);

            ShovelPatch.SetLogSource(mls);
            CustomEnemyDeaths.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()//example config setup
        {            
            var configSyncDeathAnimations = Config.Bind("Custom Death Animations",
                                        "CustomDeathAnimations",
                                        true,
                                        "Setting for attempting to sync death animations for enemies that can't normally die. If off the enemies will just despawn instead.");
            
            ConfigVariables.syncDeathAnimations = configSyncDeathAnimations.Value;
        }
    }
}
