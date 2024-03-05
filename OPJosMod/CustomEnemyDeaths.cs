using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.OneHitShovel
{
    public static class CustomEnemyDeaths
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void killHumanoid(GameObject gameObject)
        {
            mls.LogMessage($"try to kill {gameObject.name}");
            gameObject.transform.rotation = Quaternion.Euler(-90f, 0f, 90f);
            gameObject.transform.position += new Vector3(0f, 0.1f, 0f);

            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            EnemyAI enemyAIComponent = gameObject.GetComponent<EnemyAI>();
            if (enemyAIComponent != null)
            {
                mls.LogMessage("enemy ai isn't null");
                enemyAIComponent.isEnemyDead = true;
            }
            else
            {
                mls.LogMessage("enemy ai is null");
            }
        }
    }
}
