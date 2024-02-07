using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod//.ModNameHere
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class OpJosMod : BaseUnityPlugin
    {
        private const string modGUID = "OpJosMod.ModNameHere";
        private const string modName = "ModNameHere";
        private const string modVersion = "1.0.0.0"; //dont forget to update this lol

        private readonly Harmony harmony = new Harmony(modGUID);
        private static OpJosMod Instance;

        internal ManualLogSource mls;
        //update namespaces, and the strings for names
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
