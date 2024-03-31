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

            PlayerControllerBPatch.SetLogSource(mls);
            ShovelPatch.SetLogSource(mls);
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
                                        "Setting for attempting to sync death animations for enemies that can't normally die. turn off if you plan on being the only one in the lobby who is using the mod.");
            
            ConfigVariables.syncDeathAnimations = configSyncDeathAnimations.Value;
        }
    }
}
