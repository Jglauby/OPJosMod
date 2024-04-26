using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.ReviveCompany.CustomRpc;
using OPJosMod.ReviveCompany.Patches;

namespace OPJosMod.ReviveCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.ReviveCompany";
        private const string modName = "ReviveCompany";
        private const string modVersion = "0.9.4"; 

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
            ResponseHandler.SetLogSource(mls);

            PlayerControllerBPatch.SetLogSource(mls);
            ShipTeleporterPatch.SetLogSource(mls);
            StartOfRoundPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()//example config setup
        {        
            var configReviveTime = Config.Bind("Revive Time",
                                        "ReviveTime",
                                        5f,
                                        "How long it takes to revive someone");

            var configReviveTeleported = Config.Bind("Can Revive Teleported Bodies",
                                        "CanReviveTeleportedBodies",
                                        false,
                                        "Toggle for if you are able to revive dead players you teleport back to the ship");

            ConfigVariables.reviveTime = configReviveTime.Value;
            ConfigVariables.reviveTeleportedBodies = configReviveTeleported.Value;
        }
    }
}
