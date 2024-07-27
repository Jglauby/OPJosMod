using BepInEx;
using BepInEx.Configuration;
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
        private const string modVersion = "1.4.0"; 

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
            GeneralUtil.SetLogSource(mls);

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

            var configReviveHealth = Config.Bind("Health you revive with.",
                                        "HealthYouReviveWith",
                                        25,
                                        "How much health you revive with.");

            var configExtraHealthLostPerRevive = Config.Bind("Extra Health Lost Per Revive",
                            "ExtraHealthLostPerRevive",
                            5,
                            "If player is revived more than once per level, revive with this much less HP each time");

            var configLimitedRevives = Config.Bind("Limit amount of revives",
                                        "LimitAmountOfRevives",
                                        true,
                                        "do you want to have limited revives?");

            var configRevivePerLevelMultiplier = Config.Bind("Revives Per Level Multiplier",
                                                   "RevivesPerLevelMultiplier",
                                                   1.25f,
                                                   "How many revives you get per level, mulitplied by players in game. ex) value set to 2 and have 4 players then you get 8 revives.");

            var configSetAmountOfLives = Config.Bind("Set Amount of Revives",
                                        "SetAmountOfRevives",
                                        0,
                                        "Set amount of revives per level. If not at 0 This will override the revive amount being set by the (Revives Per Level Multiplier) setting");

            //var configInfiniteReviveTime = Config.Bind("Infinite Revive Time",
            //                            "InfiniteReviveTime",
            //                            false,
            //                            "Can you alwasy revive someone no longer how long they have been dead?");

            //var configRevivableTime = Config.Bind("Time Until Can't Be Revived",
            //                            "TimeUntilCantBeRevived",
            //                            120,
            //                            "How long someone can be dead for and still be revived");


            ConfigVariables.reviveTime = configReviveTime.Value;
            ConfigVariables.ReviveButton = configReviveButton.Value;
            ConfigVariables.CanPickUpBodies = configCanPickUpBodies.Value;
            ConfigVariables.DeadPlayerWeight = configDeadBodyWeight.Value;
            ConfigVariables.reviveTeleportedBodies = configReviveTeleported.Value;
            ConfigVariables.ReviveToHealth = configReviveHealth.Value;
            ConfigVariables.LimitedRevives = configLimitedRevives.Value;
            ConfigVariables.RevivesPerLevelMultiplier = configRevivePerLevelMultiplier.Value;
            ConfigVariables.HardAmountOfLives = configSetAmountOfLives.Value;
            ConfigVariables.ExtraHealthLostPerRevive = configExtraHealthLostPerRevive.Value;
            //ConfigVariables.InfiniteReviveTime = configInfiniteReviveTime.Value;
            //ConfigVariables.TimeUnitlCantBeRevived = configRevivableTime.Value;
        }
    }
}
