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
                if (AreVectorsClose(enemy.transform.position, location, 3f))
                    result = true;
            }

            return result;
        }
    }
}
