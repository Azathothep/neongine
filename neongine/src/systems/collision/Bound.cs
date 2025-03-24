using System;
using Microsoft.Xna.Framework;

namespace neongine {
    public struct Bound {
        public Vector2 Center;
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Bound(Vector2 center, float x, float y, float width, float height) {
            this.Center = center;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public static Bound Get(Shape shape, Vector2 position, Vector2 size, float rotation) {
            switch (shape.ShapeType) {
                case Shape.Type.Circle:
                    float radius = shape.Width * size.X;
                    return new Bound(position, (int)-radius, (int)-radius, (int)radius * 2, (int)radius * 2);
                default:
                    Vector2[] points = Shape.RotatePoints(shape, size, rotation);
                    
                    float xMin = float.MaxValue;
                    float xMax = float.MinValue;
                    float yMin = float.MaxValue;
                    float yMax = float.MinValue;

                    foreach (var point in points) {
                        if (point.X < xMin)
                            xMin = point.X;
                        if (point.X > xMax)
                            xMax = point.X;
                        if (point.Y < yMin)
                            yMin = point.Y;
                        if (point.Y > yMax)
                            yMax = point.Y;                    
                    }

                    return new Bound(position, xMin, yMin, xMax - xMin, yMax - yMin);
            }
        }

        public static bool IsCrossing(Bound b1, Bound b2) {
            (Bound leftestBound, Bound rightestBound) = b1.Center.X + b1.X < b2.Center.X + b2.X ? (b1, b2) : (b2, b1);
            (Bound topestBound, Bound bottomestBound) = b1.Center.Y + b1.Y < b2.Center.Y + b2.Y ? (b1, b2) : (b2, b1);

            return ((rightestBound.Center.X + rightestBound.X) <= (leftestBound.Center.X + leftestBound.Width / 2))
            && ((bottomestBound.Center.Y + bottomestBound.Y) <= (topestBound.Center.Y + topestBound.Height / 2));
        }
    }
}
