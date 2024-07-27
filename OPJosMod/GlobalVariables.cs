using System.Collections.Generic;

namespace OPJosMod.ReviveCompany
{
    public static class GlobalVariables
    {
        public static bool ModActivated = false;
        public static List<PlayerInfo> PlayerInfos = new List<PlayerInfo>();
        public static int RemainingRevives = int.MaxValue;
        public static int AmountOfTimesRevived = 0;
    }
}
