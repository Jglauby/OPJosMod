using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.OneHitShovel.CustomRpc
{
    public static class CompleteRecievedTasks
    {
        private static ManualLogSource mls;
        public static void SetLogSource(ManualLogSource logSource)
        {
            mls = logSource;
        }

        public static void ModActivated(string modName)
        {
            if (mls.SourceName == modName)
            {
                mls.LogMessage("Mod Activated");
                GlobalVariables.ModActivated = true;
            }
        }

        public static void EnemyDied(string position)
        {
            Vector3 newPosition = GeneralUtil.StringToVector3(position);
            EnemyAI enemy = CustomEnemyDeaths.findClosestEnemyAI(newPosition);

            if (enemy != null)
                CustomEnemyDeaths.KillAnyEnemy(enemy);
            else
                mls.LogMessage("couldnt find enemy to kill");
        }
    }
}
