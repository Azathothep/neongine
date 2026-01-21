using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    /// <summary>
    /// Utilities for mathematical operations
    /// </summary>
    public static class Mathf
    {
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : (value));
        }

        public const double DegToRad = Math.PI / 180;
    }
}
