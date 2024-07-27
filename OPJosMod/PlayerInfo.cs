using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.ReviveCompany
{
    public class PlayerInfo
    {
        public int PlayerId {  get; set; }

        public bool HasBeenTeleported { get; set; }

        public float TimeDiedAt { get; set; }
    }
}
