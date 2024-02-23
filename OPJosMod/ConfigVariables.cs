using System;
using UnityEngine.InputSystem;

namespace OPJosMod
{
    public static class ConfigVariables
    {
        public static float waitTimeBetweenInteractions;
        public static string startGhostModeButton;

        public static Key getButton(string buttonName)
        {
            Key key = (Key)Enum.Parse(typeof(Key), buttonName);
            return key;
        }
    }
}
