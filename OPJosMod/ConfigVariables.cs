using DunGen;
using System;

namespace OPJosMod.ReviveCompany
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
