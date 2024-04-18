using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.ReviveCompany
{
    public static class GeneralUtil
    {
        public static Vector3 StringToVector3(string str)
        {
            string[] components = str.Trim('(', ')').Split(',');

            float x = float.Parse(components[0]);
            float y = float.Parse(components[1]);
            float z = float.Parse(components[2]);

            return new Vector3(x, y, z);
        }

        public static PlayerControllerB GetClosestDeadPlayer(Vector3 position)
        {
            //implemnt this
            return StartOfRound.Instance.localPlayerController;
        }
    }
}
