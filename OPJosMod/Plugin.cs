using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.OneHitShovel.CustomRpc;
using OPJosMod.OneHitShovel.Patches;
using Unity.Netcode;

namespace OPJosMod.OneHitShovel
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.OneHitShovel";
        private const string modName = "OneHitShovel";
        private const string modVersion = "1.3.0"; 

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

            ShovelPatch.SetLogSource(mls);
            PlayerControllerBPatch.SetLogSource(mls);
            CustomEnemyDeaths.SetLogSource(mls);
            HUDManagerPatchForRPC.SetLogSource(mls);
            RpcMessageHandler.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()//example config setup
        {            
            var configSyncDeathAnimations = Config.Bind("Sync Death Animations",
                                        "SyncDeathAnimations",
                                        true,
                                        "Setting for attempting to sync death animations for enemies that can't normally die. This only works if everyone has the mod. If its just you turn this off and the enemies will just despawn instead.");
            
            ConfigVariables.syncDeathAnimations = configSyncDeathAnimations.Value;
        }
    }
}
