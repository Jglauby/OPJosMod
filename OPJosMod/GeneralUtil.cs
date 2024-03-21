﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod.LagJutsu.Utils
{
    public static class GeneralUtil
    {
        public static bool AreVectorsClose(Vector3 v1, Vector3 v2, float threshold)
        {
            // Calculate absolute differences for x, y, and z components
            float dx = Mathf.Abs(v1.x - v2.x);
            float dy = Mathf.Abs(v1.y - v2.y);
            float dz = Mathf.Abs(v1.z - v2.z);

            // Check if all differences are within the threshold
            return dx <= threshold && dy <= threshold && dz <= threshold;
        }
    }
}
