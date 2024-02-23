using BepInEx;
using System;
using System.Reflection;
using UnityEngine.InputSystem;

namespace OPJosMod
{
    public static class ConfigVariables
    {
        public static float defaultSprintMultiplier;
        public static float defaultMaxSprintSpeed;
        public static float walkMultiplier;
        public static float maxWalkSpeed;
        public static string flashTimeButton;

        public static Key getFlashTimeButton()
        {
            Key key = (Key)Enum.Parse(typeof(Key), flashTimeButton);
            return key;
        }
    }
}
