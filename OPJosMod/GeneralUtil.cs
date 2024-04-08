using DunGen;
using HarmonyLib;
using OPJosMod.LagJutsu.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OPJosMod.LagJutsu.Utils
{
    public static class GeneralUtil
    {
        public static bool AreVectorsClose(Vector3 v1, Vector3 v2, float threshold)
        {
            // Calculate absolute differences for x, y, and z components
            float dx = Mathf.Abs(v1.x - v2.x);
            //float dy = Mathf.Abs(v1.y - v2.y);
            float dz = Mathf.Abs(v1.z - v2.z);

            // Check if all differences are within the threshold
            return dx <= threshold && dz <= threshold;
        }

        public static bool ExistsCloseEnemy(Vector3 location)
        {
            bool result = false;           

            foreach (EnemyAI enemy in EnemyAIPatch.allEnemies)
            {
                if (AreVectorsClose(enemy.transform.position, location, 2f))
                    result = true;
            }

            return result;
        }

        public static int GetGameVersion()
        {
            try
            {
                var version = GameNetworkManager.Instance.gameVersionNum;
                return version;
            }
            catch { }

            return 0;
        }

        private static bool hasSetupVersion = false;
        public static void SetupForVersion(OpJosMod instance, Harmony harmony)
        {
            if (hasSetupVersion)
                return;

            var version = GetGameVersion();

            if (version == 0)
                return;

            if (version >= 50)
            {
                instance.mls.LogMessage("version is >= 50");
                RadMechAIPatch.SetLogSource(instance.mls);
                harmony.PatchAll(typeof(RadMechAIPatch));
            }

            hasSetupVersion = true;
        }
    }
}
