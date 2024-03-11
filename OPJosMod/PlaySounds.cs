using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod
{
    public static class PlaySounds
    {
        private static WhoopieCushionItem whoopieItem;
        private static AudioClip[] fartAudios;

        private static Vector3 lastPositionAtFart;
        private static int timesPlayingInOneSpot;

        public static void InitializePlaySounds()
        {
            whoopieItem = Resources.FindObjectsOfTypeAll<WhoopieCushionItem>()[0];
            fartAudios = whoopieItem.fartAudios;           
        }

        public static void PlayFart(PlayerControllerB player)
        {
            InitializePlaySounds();
            var audioSource = player.itemAudio;

            if (Vector3.Distance(lastPositionAtFart, player.transform.position) > 2f)
            {
                timesPlayingInOneSpot = 0;
            }
            timesPlayingInOneSpot++;
            lastPositionAtFart = player.transform.position;
            RoundManager.PlayRandomClip(audioSource, fartAudios, randomize: true, 1f, -1);
            RoundManager.Instance.PlayAudibleNoise(player.transform.position, 8f, 0.8f, timesPlayingInOneSpot, false && StartOfRound.Instance.hangarDoorsClosed, 101158);
        }
    }
}
