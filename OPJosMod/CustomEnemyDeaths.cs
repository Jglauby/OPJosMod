using BepInEx.Logging;
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

            Renderer renderer = gameObject.GetComponent<Renderer>();
            float moveUpDistance = (renderer != null) ? renderer.bounds.size.x * 0.1f : 0.1f;
            gameObject.transform.position += new Vector3(0f, moveUpDistance, 0f);

            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            EnemyAI enemyAIComponent = findClosestEnemyAI(gameObject);
            if (enemyAIComponent != null)
            {
                mls.LogMessage("enemy ai isn't null");
                enemyAIComponent.isEnemyDead = true;
                enemyAIComponent.creatureAnimator.enabled = false;
            }
            else
            {
                mls.LogMessage("enemy ai is null");
            }
        }

        private static EnemyAI findClosestEnemyAI(GameObject gameObject)
        {
            EnemyAI resultingEnemy = null;

            EnemyAI[] enemyAIs = Object.FindObjectsOfType<EnemyAI>(); 
            float closestDistance = Mathf.Infinity; 

            foreach (EnemyAI enemyAI in enemyAIs)
            {
                float distance = Vector3.Distance(gameObject.transform.position, enemyAI.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance; 
                    resultingEnemy = enemyAI; 
                }
            }

            if (resultingEnemy != null)
            {
                Debug.Log("Closest EnemyAI found: " + resultingEnemy.name);
            }
            else
            {
                Debug.Log("No EnemyAI found in the scene.");
            }

            return resultingEnemy;
        }
    }
}
