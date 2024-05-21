using System.Collections.Generic;

namespace OPJosMod.ReviveCompany
{
    public static class GlobalVariables
    {
        public static bool ModActivated = false;
        public static List<int> DeadBodiesTeleported = new List<int>();
        public static int RemainingRevives = int.MaxValue;
    }
}
