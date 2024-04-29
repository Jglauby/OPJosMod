using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.MoreEnemies
{
    public static class GlobalVariables
    {
        public static bool ModActivated = false;

        public static AnimationCurve OGenemySpawnChanceThroughoutDay = null;
        public static AnimationCurve OGoutsideEnemySpawnChanceThroughDay = null;
        public static AnimationCurve OGdaytimeEnemySpawnChanceThroughDay = null;
        public static float OGcurrentMaxInsidePower = 0f;
        public static int OGmaxEnemyPowerCount = 0;
        public static float OGcurrentMaxOutsidePower = 0f;
        public static int OGmaxOutsideEnemyPowerCount = 0;
    }
}
