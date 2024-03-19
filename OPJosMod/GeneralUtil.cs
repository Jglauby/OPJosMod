using BepInEx.Logging;
using OPJosMod.HideNSeek.Utils;
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
    }
}
