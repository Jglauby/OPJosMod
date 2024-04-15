using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.BreadCrumbs.Patches;
using UnityEngine.InputSystem;

namespace OPJosMod.BreadCrumbs
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.BreadCrumbs";
        private const string modName = "BreadCrumbs";
        private const string modVersion = "1.1.0";

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
            EntranceTeleportPatch.SetLogSource(mls);
            ShipTeleporterPatch.SetLogSource(mls);

            harmony.PatchAll();
        }

        private void setupConfig()
        {
            var configRetraceButton = Config.Bind("Retrace Steps Button",
                                        "RetraceStepsButton",
                                        Key.H,
                                        "Button used to start walking back to the front door");

            ConfigVariables.retraceButton = configRetraceButton.Value;
        }
    }
}
