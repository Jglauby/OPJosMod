using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OPJosMod.GodMode.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.GodMode.Enums
{
    public enum __RpcExecStage
    {
        None,
        Server,
        Client
    }
}
