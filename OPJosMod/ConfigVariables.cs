using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.LagJutsu
{
    public static class ConfigVariables //example config variables class
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
