using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPJosMod.OneHitShovel
{
    public static class Constants
    {
        //FGiantModelContainer -> forest giant
        //SpringManModel -> coil head
        //PufferModel -> puffer
        //DressGirlModel -> ghost girl
        //BoneEast -> blob
        //Bone.003 -> earth worm
        //MeshContainer -> jester
        public static List<string> humanoidNames = new List<string> { "SpringManModel", "DressGirlModel", "MeshContainer", 
            "SpringMan(Clone)", "DressGirl(Clone)", "JesterEnemy(Clone)" };
        
        public static string[] fourLeggedNames = new string[] { "PufferModel", "PufferEnemy(Clone)" };
        
        public static string[] dieInPlaceNames = new string[] { "Bone.", "Bone", "SandWorm(Clone)" };
        
        public static string[] slimeNames = new string[] { "BoneEast", "BoneNorth", "BoneSouth", "BoneWest",
                "BoneNorthWest", "BoneNorthEast", "BoneSouthEast", "BoneSouthWest", "Center", "Blob(Clone)"};

        public static string[] mechNames = new string[] { "ClawTrigger", "RadMechEnemy(Clone)" };
    }
}
