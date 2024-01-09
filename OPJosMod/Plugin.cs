using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GodMode.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.GodMode
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.GodMode";
        private const string modName = "GodMode";
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

            harmony.PatchAll(typeof(OpJosMod));

            Patches.PlayerControllerBPatch.SetLogSource(mls);
            harmony.PatchAll(typeof(PlayerControllerBPatch));
        }
    }
}
