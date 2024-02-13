using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GhostMode.Enemy.Patches;
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
        private const string modVersion = "1.7.5";

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

            PlayerControllerBPatch.SetLogSource(mls);
            StartOfRoundPatch.SetLogSource(mls);
            EnemyAIPatch.SetLogSource(mls);
            HUDManagerPatch.SetLogSource(mls);
            CentipedeAIPatch.SetLogSource(mls);
            MouthDogAIPatch.SetLogSource(mls);
            ForestGiantAIPatch.SetLogSource(mls);
            SandSpiderAIPatch.SetLogSource(mls);

            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
            harmony.PatchAll(typeof (CentipedeAIPatch));
            harmony.PatchAll(typeof(MouthDogAIPatch));
            harmony.PatchAll(typeof(ForestGiantAIPatch));
            harmony.PatchAll(typeof(SandSpiderAIPatch));
        }
    }
}
