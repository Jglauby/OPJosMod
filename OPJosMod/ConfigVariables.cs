using System;
using UnityEngine.InputSystem;

namespace OPJosMod
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
