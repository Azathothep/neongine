using System;
using Microsoft.Xna.Framework;

namespace neongine {
    public struct Shape
    {
        private Vector2[] m_Vertices;
        public Vector2[] Vertices => m_Vertices;

        public Shape(Vector2[] vertices) {
            m_Vertices = vertices;
        }

        public Shape(Geometry geometry, float rotation, Vector2 scale) {
            switch (geometry.Type) {
                case GeometryType.Circle:
                    m_Vertices = BuildCircle(geometry.Width * scale.X);
                    break;
                case GeometryType.Rectangle:
                    m_Vertices = BuildRectangle(geometry.Width * scale.X, geometry.Height * scale.Y, rotation);
                    break;
                default:
                    m_Vertices = null;
                    break;
            }
        }

        private Vector2[] BuildCircle(float width) {
            Vector2[] vertices = new Vector2[1];
            vertices[0].X = width;

            return vertices;
        }

        private Vector2[] BuildRectangle(float width, float height, float rotation) {
            Vector2[] points = new Vector2[4];

            float x = width / 2;
            float y = height / 2;

            if (rotation == 0.0f) {
                points[0] = new Vector2(-x, y);
                points[1] = new Vector2(x, y);
                points[2] = new Vector2(x, -y);
                points[3] = new Vector2(-x, -y);

                return points;
            }

            double rad = float.DegreesToRadians(-rotation);
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            points[0] = new Vector2((- x) * cos + y * sin, - (- x) * sin + y * cos);
            points[1] = new Vector2(x * cos + y * sin, - x * sin + y * cos);
            points[2] = -points[0];
            points[3] = -points[1];

            return points;
        }
    }
}
