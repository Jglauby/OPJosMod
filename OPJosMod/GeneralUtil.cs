using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.OneHitShovel
{
    public static class GeneralUtil
    {
        public static Vector3 StringToVector3(string s)
        {
            // Remove parentheses and split string by commas
            string[] components = s.Trim('(', ')').Split(',');

            // Parse components as floats
            float x = float.Parse(components[0]);
            float y = float.Parse(components[1]);
            float z = float.Parse(components[2]);

            // Create a new Vector3
            return new Vector3(x, y, z);
        }
    }
}
