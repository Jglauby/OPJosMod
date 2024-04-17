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
using Object = UnityEngine.Object;

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
            //mls.LogMessage($"mod activated:{GlobalVariables.ModActivated}");
            if (!GlobalVariables.ModActivated)
                return;

            try
            {
                __instance.shovelHitForce = 30;
                GameObject hitObject = null;

                var previousPlayerHeldBy = ReflectionUtils.GetFieldValue<PlayerControllerB>(__instance, "previousPlayerHeldBy");
                var objectsHitByShovel = ReflectionUtils.GetFieldValue<RaycastHit[]>(__instance, "objectsHitByShovel");
                var objectsHitByShovelList = ReflectionUtils.GetFieldValue<List<RaycastHit>>(__instance, "objectsHitByShovelList");
                var shovelMask = ReflectionUtils.GetFieldValue<int>(__instance, "shovelMask");

                objectsHitByShovel = Physics.SphereCastAll(previousPlayerHeldBy.gameplayCamera.transform.position + previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.8f, previousPlayerHeldBy.gameplayCamera.transform.forward, 1.5f, shovelMask, QueryTriggerInteraction.Collide);
                objectsHitByShovelList = objectsHitByShovel.OrderBy(x => x.distance).ToList();
                for (int i = 0; i < objectsHitByShovelList.Count; i++)
                {
                    RaycastHit val = objectsHitByShovelList[i];

                    if (val.transform.TryGetComponent<IHittable>(out var component))
                    {
                        if (!val.transform.gameObject.name.Contains("Player"))
                        {
                            mls.LogMessage("Hit object: " + val.transform.gameObject.name);
                            hitObject = val.transform.gameObject;
                        }                           
                    }
                }

                if (hitObject != null)
                {
                    CustomEnemyDeaths.KillGameObjectEnemy(hitObject);
                    CustomEnemyDeaths.updateLocationOnServer(hitObject);
                }

                var hitMech = checkIfHitMech();
                if (hitMech != null)
                    StartOfRound.Instance.localPlayerController.StartCoroutine(CustomEnemyDeaths.KillMech((Vector3)hitMech, true));
            }
            catch (Exception e)
            {
                mls.LogError(e);
            }
        }

        private static Vector3? checkIfHitMech()
        {
            try
            {
                var player = StartOfRound.Instance.localPlayerController;
                RadMechAI[] radMechs = Object.FindObjectsOfType<RadMechAI>();

                if (radMechs.Length == 0)
                    return null;

                RadMechAI closestMech = null;
                float minDistance = Mathf.Infinity;
                foreach (var mech in radMechs)
                {
                    Vector3 directionToMech = mech.transform.position - player.transform.position;
                    if (Vector3.Dot(player.transform.forward, directionToMech.normalized) > 0.5f)
                    {
                        float distanceToMech = directionToMech.magnitude;
                        if (distanceToMech <= 3f)
                        {
                            if (distanceToMech < minDistance)
                            {
                                minDistance = distanceToMech;
                                closestMech = mech;
                            }
                        }
                    }
                }

                if (closestMech == null)
                    return null;

                return closestMech.transform.position;
            }
            catch { return null; }
        }
    }
}
