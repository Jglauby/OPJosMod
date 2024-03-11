using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.Patches
{
    [HarmonyPatch(typeof(WhoopieCushionItem), "Fart")]
    public class WhoopieCushionItemPatch
    {
        public static AudioSource whoopieCushionAudio;

        public static AudioClip[] fartAudios;

        [HarmonyPatch("__initializeVariables")]
        [HarmonyPrefix]
        public static void initializePatch(ref AudioClip[] ___fartAudios)
        {
            fartAudios = ___fartAudios;
        }
    }
}
