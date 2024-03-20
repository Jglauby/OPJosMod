using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.SupahNinja.Utils
{
    public class GeneralUtils
    {
        public static bool twoPointsAreClose(Vector3 point1,  Vector3 point2, float closeThreshold)
        {
            if (point1 != null && point2 != null)
            {
                float distance = Vector3.Distance(point1, point2);

                if (distance <= closeThreshold)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public static string simplifyObjectNames(string ogName)
        {
            var objectName = ogName;
            int index = objectName.IndexOf("(");
            if (index != -1)
                objectName = objectName.Substring(0, index).Trim();

            return objectName;
        }

        public static bool playerIsCrouching()
        {
            var player = StartOfRound.Instance.localPlayerController;

            if (player.isCrouching || ConfigVariables.alwaysSneak)
                return true;

            return false;
        }
    }
}
