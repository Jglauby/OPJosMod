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
        public static string[] humanoidNames = new string[] { "SpringManModel", "FGiantModelContainer", "DressGirlModel", "MeshContainer" };
        
        public static string[] fourLeggedNames = new string[] { "PufferModel" };
        
        public static string[] dieInPlaceNames = new string[] { "Bone.", "Bone" };
        
        public static string[] slimeNames = new string[] { "BoneEast", "BoneNorth", "BoneSouth", "BoneWest",
                "BoneNorthWest", "BoneNorthEast", "BoneSouthEast", "BoneSouthWest", "Center"};
    }
}
