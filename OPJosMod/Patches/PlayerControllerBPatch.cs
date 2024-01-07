using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("Update")]//if can use typeof(PlayerControllerB.Update)
        [HarmonyPostfix]
        static void patchUpdate(ref float ___sprintMeter)
        {
            ___sprintMeter = 1f;
        }
    }
}
