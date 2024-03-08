using BepInEx.Logging;
using OPJosMod.OneHitShovel.Utils;
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
                stopAllSounds(enemyAIComponent);
            }
            else
            {
                mls.LogMessage("enemy ai is null");
            }

            updateLocationOnServer(enemyAIComponent);
        }

        public static void killInPlace(GameObject gameObject)
        {
            mls.LogMessage($"try to kill {gameObject.name}");

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
                stopAllSounds(enemyAIComponent);
            }
            else
            {
                mls.LogMessage("enemy ai is null");
            }

            updateLocationOnServer(enemyAIComponent);
        }

        public static void killFourLegged(GameObject gameObject)
        {
            mls.LogMessage($"try to kill {gameObject.name}");
            gameObject.transform.rotation = Quaternion.Euler(180f, 0f, 0f);

            Renderer renderer = gameObject.GetComponent<Renderer>();
            float moveUpDistance = (renderer != null) ? renderer.bounds.size.y * 0.1f : 1f; 
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
                stopAllSounds(enemyAIComponent);
            }
            else
            {
                mls.LogMessage("enemy ai is null");
            }

            updateLocationOnServer(enemyAIComponent);
        }

        public static void killSlime(GameObject gameObject)
        {
            mls.LogMessage($"try to kill {gameObject.name}");

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

            Object.Destroy(enemyAIComponent);
            stopAllSounds(enemyAIComponent);

            foreach(string name in Constants.slimeNames)
            {
                destroySlimePart(gameObject, name);       
            }

            updateLocationOnServer(enemyAIComponent);
        }

        private static void updateLocationOnServer(EnemyAI enemy)
        {
            enemy.KillEnemyServerRpc(false);

            short rotation = (short)enemy.transform.rotation.eulerAngles.y;
            ReflectionUtils.InvokeMethod(enemy, "UpdateEnemyRotationServerRpc", new object[] { rotation });

            Vector3 postion = enemy.transform.position;
            ReflectionUtils.InvokeMethod(enemy, "UpdateEnemyPositionServerRpc", new object[] { postion });
        }

        private static void stopAllSounds(EnemyAI enemy)
        {
            AudioSource[] objectAudioSources = enemy.GetComponentsInChildren<AudioSource>();

            foreach (AudioSource audioSource in objectAudioSources)
            {
                audioSource.Stop();
            }
        }

        private static void destroySlimePart(GameObject gameObject, string name)
        {
            GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
            GameObject closestObject = null;
            float closestDistance = Mathf.Infinity;

            foreach (GameObject obj in allGameObjects)
            {
                if (obj.name == name)
                {
                    float distance = Vector3.Distance(gameObject.transform.position, obj.transform.position);
                    if (distance < closestDistance && distance < 10)
                    {
                        closestDistance = distance;
                        closestObject = obj;
                    }
                }
            }

            if (closestObject != null)
            {
                mls.LogMessage("Closest Enemy with name '" + name + "' found: " + closestObject.name);
            }
            else
            {
                mls.LogMessage("No Enemy with name '" + name + "' found in the scene.");
            }

            
            Object.Destroy(closestObject);
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
