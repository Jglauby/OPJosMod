using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.ReviveCompany.CustomRpc;
using OPJosMod.ReviveCompany.Patches;
using UnityEngine.InputSystem;

namespace OPJosMod.ReviveCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.ReviveCompany";
        private const string modName = "ReviveCompany";
        private const string modVersion = "1.0.1"; 

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
            RagdollGrabbableObjectPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()//example config setup
        {        
            var configReviveTime = Config.Bind("Revive Time",
                                        "ReviveTime",
                                        5f,
                                        "How long it takes to revive someone");

            var configReviveButton = Config.Bind("ReviveButton",
                                        "ReviveButton",
                                        Key.R,
                                        "Button Used to Revive Players");

            var configCanPickUpBodies = Config.Bind("Can Pick Up Bodies",
                                       "CanPickUpBodies",
                                       true,
                                       "Toggle if you can pick up bodies.");

            var configDeadBodyWeight = Config.Bind("Dead Body Weight Multiplier",
                                       "DeadBodyWeight",
                                       3.25f,
                                       "How heavy are the dead players.");

            var configReviveTeleported = Config.Bind("Can Revive Teleported Bodies",
                                        "CanReviveTeleportedBodies",
                                        false,
                                        "Toggle for if you are able to revive dead players you teleport back to the ship");

            ConfigVariables.reviveTime = configReviveTime.Value;
            ConfigVariables.ReviveButton = configReviveButton.Value;
            ConfigVariables.CanPickUpBodies = configCanPickUpBodies.Value;
            ConfigVariables.DeadPlayerWeight = configDeadBodyWeight.Value;
            ConfigVariables.reviveTeleportedBodies = configReviveTeleported.Value;
        }
    }
}
