using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace neongine
{
    public enum GeometryType {
        Circle,
        Rectangle
    }

    public struct Geometry
    {
        [Serialize]
        public GeometryType Type;

        [Serialize]
        public float Width;

        [Serialize]
        public float Height;

        public Geometry(GeometryType type) : this(type, 1, 1) {

        }

        public Geometry(GeometryType type, float width) : this(type, width, width) {

        }

        public Geometry(GeometryType type, float width, float height) {
            Type = type;
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj)
        {
            return obj is Geometry other && other.Type == this.Type && other.Width == this.Width && other.Height == this.Height;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Vector2[] RotatePoints(Geometry geometry, Vector2 size, float rotation) {
            switch (geometry.Type)
            {
                case GeometryType.Circle:
                    return null;
                case GeometryType.Rectangle:
                    return RotateRectanglePoints(geometry.Width * size.X, geometry.Height * size.Y, rotation);
                default:
                    break;
            }

            return null;
        }

        private static Vector2[] RotateRectanglePoints(float width, float height, float rotation) {
            Vector2[] points = new Vector2[4];
            
            double rad = float.DegreesToRadians(rotation);
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            float right = width / 2;
            float bottom = height / 2;
            float x = -right;
            float y = -bottom;

            points[0] = new Vector2(x * cos - y * sin, y * cos + x * sin);
            points[1] = new Vector2(right * cos - y * sin, y * cos + right * sin);
            points[2] = new Vector2(x * cos - bottom * sin, bottom * cos + x * sin);
            points[3] = new Vector2(right * cos - bottom * sin, bottom * cos + right * sin);

            return points;
        }
    }
}
