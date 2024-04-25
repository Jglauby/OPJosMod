using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.MoreEnemies
{
    public static class ConfigVariables
    {
        public static float defaultSprintMultiplier;
        public static string flashTimeButton;

        public static Key getFlashTimeButton()
        {
            Key key = (Key)Enum.Parse(typeof(Key), flashTimeButton);
            return key;
        }
    }
}
