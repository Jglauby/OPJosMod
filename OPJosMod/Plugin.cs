using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GhostMode.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.GhostMode
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.GhostMode";
        private const string modName = "GhostMode";
        private const string modVersion = "1.0.0.0";

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

            Patches.PlayerControllerBPatch.SetLogSource(mls);
            Patches.StartOfRoundPatch.SetLogSource(mls);
            Patches.EnemyAIPatch.SetLogSource(mls);

            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
        }
    }
}
