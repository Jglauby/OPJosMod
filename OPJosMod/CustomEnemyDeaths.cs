using BepInEx.Logging;
using OPJosMod.OneHitShovel.CustomRpc;
using OPJosMod.OneHitShovel.Utils;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

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

            EnemyAI enemyAIComponent = findClosestEnemyAI(gameObject.transform.position);
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
        }

        public static void killInPlace(GameObject gameObject)
        {
            mls.LogMessage($"try to kill {gameObject.name}");

            Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            EnemyAI enemyAIComponent = findClosestEnemyAI(gameObject.transform.position);
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

            EnemyAI enemyAIComponent = findClosestEnemyAI(gameObject.transform.position);
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
        }

        public static void killSlime(GameObject gameObject)
        {
            mls.LogMessage($"try to kill {gameObject.name}");

            EnemyAI enemyAIComponent = findClosestEnemyAI(gameObject.transform.position);
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
        }

        public static void updateLocationOnServer(GameObject gameObject)
        {
            var enemyAi = findClosestEnemyAI(gameObject.transform.position);

            if (enemyAi != null)
                enemyAi.KillEnemyServerRpc(false);
            else
                mls.LogMessage("no enemy to kill");

            //send message to update death for other players with mod
            if (ConfigVariables.syncDeathAnimations)
            {
                var rpcMessage = new RpcMessage($"EnemyDied:{gameObject.transform.position}", (int)StartOfRound.Instance.localPlayerController.playerClientId, MessageCodes.Request);
                RpcMessageHandler.SendRpcMessage(rpcMessage);
            }
            else
            {
                enemyAi.KillEnemyServerRpc(true);
            }

            //handle case where other players don't have mod
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
                    if (distance < closestDistance && distance < 15)
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

        public static EnemyAI findClosestEnemyAI(Vector3 location)
        {
            EnemyAI resultingEnemy = null;

            EnemyAI[] enemyAIs = Object.FindObjectsOfType<EnemyAI>(); 
            float closestDistance = Mathf.Infinity; 

            foreach (EnemyAI enemyAI in enemyAIs)
            {
                float distance = Vector3.Distance(location, enemyAI.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance; 
                    resultingEnemy = enemyAI; 
                }
            }

            if (resultingEnemy != null)
            {
                mls.LogMessage("Closest EnemyAI found: " + resultingEnemy.name);
            }
            else
            {
                mls.LogError("No EnemyAI found in the scene.");
            }

            return resultingEnemy;
        }

        public static void KillAnyEnemy(EnemyAI enemy)
        {
            mls.LogMessage($"enemy died {enemy.name}");
            GameObject gameObject = enemy.gameObject;

            KillGameObjectEnemy(gameObject);
        }

        public static void KillGameObjectEnemy(GameObject gameObject)
        {           
            if (gameObject != null && gameObject.name != "Player")
            {
                mls.LogMessage($"kill game object with name: {gameObject.name}");
                if (Constants.humanoidNames.Contains(gameObject.name))
                {
                    CustomEnemyDeaths.killHumanoid(gameObject);
                }

                if (Constants.fourLeggedNames.Contains(gameObject.name))
                {
                    CustomEnemyDeaths.killFourLegged(gameObject);
                }

                if (Constants.slimeNames.Contains(gameObject.name))
                {
                    CustomEnemyDeaths.killSlime(gameObject);
                }

                if (gameObject.name != null)
                {
                    int dotIndex = gameObject.name.LastIndexOf('.');
                    if (dotIndex != -1)
                    {
                        string trimmedName = gameObject.name.Substring(0, dotIndex + 1);
                        if (Constants.dieInPlaceNames.Contains(trimmedName))
                        {
                            CustomEnemyDeaths.killInPlace(gameObject);
                        }
                    }
                }
            }
            else
            {
                mls.LogError("game object was null");
            }
        }
    }
}
