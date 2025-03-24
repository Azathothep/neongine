using System;
using Microsoft.Xna.Framework;

namespace neongine
{
    public struct Shape
    {
        public enum Type {
            Circle,
            Rectangle
        }

        [Serialize]
        public Type ShapeType;

        [Serialize]
        public float Width;

        [Serialize]
        public float Height;

        public Shape(Type shapeType, float width = 1, float height = 1) {
            ShapeType = shapeType;
            Width = width;
            Height = height;
        }

        public static Vector2[] RotatePoints(Shape shape, Vector2 size, float rotation) {
            switch (shape.ShapeType)
            {
                case Shape.Type.Circle:
                    return null;
                case Shape.Type.Rectangle:
                    return RotateRectanglePoints(shape.Width * size.X, shape.Height * size.Y, rotation);
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
