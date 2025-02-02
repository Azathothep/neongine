using System;
using Microsoft.Xna.Framework;

namespace neongine
{
    public static class VectorExtension
    {
        public static Vector2 Rotate(this Vector2 v, double degrees)
        {
            return v.RotateRadians(degrees * Mathf.DegToRad);
        }

        public static Vector2 RotateRadians(this Vector2 v, double radians)
        {
            var ca = (float)Math.Cos(radians);
            var sa = (float)Math.Sin(radians);
            return new Vector2(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
        }

        public static Vector3 Rotate(this Vector3 v, double degrees)
        {
            return v.RotateRadians(degrees * Mathf.DegToRad);
        }

        public static Vector3 RotateRadians(this Vector3 v, double radians)
        {
            var ca = (float)Math.Cos(radians);
            var sa = (float)Math.Sin(radians);
            return new Vector3(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y, v.Z);
        }

        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0.0f);
        }
    }

}