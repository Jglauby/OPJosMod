using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using OPJosMod.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace OPJosMod.GhostMode.Patches
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
        private static bool hitShovelPatch(Shovel __instance)
        {
            if (ConfigVariables.OPness == OPnessModes.Unrestricted)
                return true;

            try
            {
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

                //if hitting a player
                if (hitObject != null && hitObject.name.Contains("Player"))
                {
                    mls.LogMessage("dont actaully hit the player");
                    return false;
                }
            }
            catch (Exception e)
            {
                mls.LogError(e);
                return true;
            }

            return true;
        }
    }
}
