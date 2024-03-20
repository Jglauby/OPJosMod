using BepInEx.Logging;
using GameNetcodeStuff;
using OPJosMod.HideNSeek.Config;
using OPJosMod.HideNSeek.Utils;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace OPJosMod
{
    public static class GeneralUtil
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void spawnItemAtLocation(BuyableItems item, Vector3 location)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                if (item != BuyableItems.None)
                {
                    Terminal terminal = ReflectionUtils.GetFieldValue<Terminal>(HUDManager.Instance, "terminalScript");
                    GameObject obj = Object.Instantiate(terminal.buyableItemsList[(int)item].spawnPrefab, location, Quaternion.identity, StartOfRound.Instance.localPlayerController.playersManager.propsContainer);

                    obj.GetComponent<GrabbableObject>().fallTime = 0f;
                    obj.GetComponent<NetworkObject>().Spawn();
                }
            }
            else
            {
                mls.LogError($"tried to spawn item: {item} but can't as this is not the host");
            }
        }

        public static void spawnHiderItem(int seekerId)
        {
            if (GameNetworkManager.Instance.isHostingGame)
            {
                var allPlayers = getAllPlayersConneced();

                foreach (PlayerControllerB player in allPlayers)
                {
                    if ((int)player.playerClientId != seekerId)
                    {
                        mls.LogMessage($"spawn hider item at {player.transform.position}");
                        spawnItemAtLocation(ConfigVariables.hiderItem, player.transform.position);
                    }
                    else
                        mls.LogMessage($"dont spawn hider item at this players position,{player.playerClientId} they are seeker");
                }
            }
        }

        //properlly grabs connected players even if you have more company on
        public static int getTotalPlayersCount()
        {
            var allPlayers = RoundManager.Instance.playersManager.allPlayerScripts.Where(x => !x.playerUsername.Contains("Player #")).ToArray(); 
            
            return allPlayers.Length;
        }

        public static PlayerControllerB[] getAllPlayersConneced()
        {
            var allPlayers = RoundManager.Instance.playersManager.allPlayerScripts.Where(x => !x.playerUsername.Contains("Player #")).ToArray();

            return allPlayers;
        }
    }
}
