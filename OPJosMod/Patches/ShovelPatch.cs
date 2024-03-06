using BepInEx.Logging;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.OneHitShovel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.OneHitShovel.Patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal class ShovelPatch
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPrefix]
        private static void hitShovelPatch(Shovel __instance)
        {
            __instance.shovelHitForce = 30;
            GameObject hitObject = null;

            var previousPlayerHeldBy = ReflectionUtils.GetFieldValue<PlayerControllerB>(__instance, "previousPlayerHeldBy");
            var objectsHitByShovel = ReflectionUtils.GetFieldValue<RaycastHit[]>(__instance, "objectsHitByShovel");
            var objectsHitByShovelList = ReflectionUtils.GetFieldValue<List<RaycastHit>>(__instance, "objectsHitByShovelList");
            var shovelMask = ReflectionUtils.GetFieldValue<int>(__instance, "shovelMask");

            previousPlayerHeldBy.activatingItem = false;

            previousPlayerHeldBy.twoHanded = false;
            objectsHitByShovel = Physics.SphereCastAll(previousPlayerHeldBy.gameplayCamera.transform.position + previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.8f, previousPlayerHeldBy.gameplayCamera.transform.forward, 1.5f, shovelMask, (QueryTriggerInteraction)2);
            objectsHitByShovelList = objectsHitByShovel.OrderBy(x => x.distance).ToList();
            for (int i = 0; i < objectsHitByShovelList.Count; i++)
            {
                RaycastHit val = objectsHitByShovelList[i];

                if (val.transform.TryGetComponent<IHittable>(out var component))
                {
                    mls.LogMessage("Hit object: " + val.transform.gameObject.name);
                    hitObject = val.transform.gameObject;
                }
            }

            //FGiantModelContainer -> forest giant
            //SpringManModel -> coil head
            //PufferModel -> puffer
            //DressGirlModel -> ghost girl
            //BoneEast -> blob
            //Bone.003 -> earth worm
            //MeshContainer -> jester
            string[] humanoidNames = new string[] { "SpringManModel", "FGiantModelContainer", "DressGirlModel", "MeshContainer" };

            string[] fourLeggedNames = new string[] { "PufferModel" };

            string[] slimeNames = new string[] { "BoneEast", "BoneNorth", "BoneSouth", "BoneWest",
                "BoneNorthWest", "BoneNorthEast", "BoneSouthEast", "BoneSouthWest", "Center"};

            if (hitObject != null && hitObject.name != "Player")
            {
                if (humanoidNames.Contains(hitObject.name))
                {
                    CustomEnemyDeaths.killHumanoid(hitObject);
                }

                if (fourLeggedNames.Contains(hitObject.name))
                {
                    CustomEnemyDeaths.killFourLegged(hitObject);
                }

                if (slimeNames.Contains(hitObject.name))
                {
                    CustomEnemyDeaths.killSlime(hitObject);
                }
            }
        }
    }
}
