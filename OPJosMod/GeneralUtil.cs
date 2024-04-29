using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.MoreEnemies
{
    public static class GeneralUtil
    {
        public static void ResetSpawnRatesForLevel()
        {
            if (GlobalVariables.OGenemySpawnChanceThroughoutDay != null &&
                GlobalVariables.OGmaxEnemyPowerCount != 0 &&
                GlobalVariables.OGoutsideEnemySpawnChanceThroughDay != null &&
                GlobalVariables.OGdaytimeEnemySpawnChanceThroughDay != null)
            {
                RoundManager.Instance.currentLevel.enemySpawnChanceThroughoutDay = GlobalVariables.OGenemySpawnChanceThroughoutDay;
                RoundManager.Instance.currentMaxInsidePower = GlobalVariables.OGcurrentMaxInsidePower;
                RoundManager.Instance.currentLevel.maxEnemyPowerCount = GlobalVariables.OGmaxEnemyPowerCount;
                RoundManager.Instance.currentLevel.outsideEnemySpawnChanceThroughDay = GlobalVariables.OGoutsideEnemySpawnChanceThroughDay;
                RoundManager.Instance.currentLevel.daytimeEnemySpawnChanceThroughDay = GlobalVariables.OGdaytimeEnemySpawnChanceThroughDay;
                RoundManager.Instance.currentMaxOutsidePower = GlobalVariables.OGcurrentMaxOutsidePower;
                RoundManager.Instance.currentLevel.maxOutsideEnemyPowerCount = GlobalVariables.OGmaxOutsideEnemyPowerCount;
            }
            else
            {
                Debug.WriteLine("MOREENEMIES: didnt reset spawnrates to orignal spawnrates as orignal spawnrates werent set yet.");
            }
        }
    }
}
