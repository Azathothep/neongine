using System;
using Microsoft.Xna.Framework;

namespace neongine {
    [DoNotSerialize]
    public class Shape : IEquatable<Shape>
    {
        [Serialize]
        private Vector2[] m_Vertices;
        public Vector2[] Vertices => m_Vertices;

        public bool IsPolygon => m_Vertices.Length > 2;

        public float Radius => IsPolygon ? 0.0f : m_Vertices[1].X;

        public Shape(Vector2[] vertices) {
            m_Vertices = vertices;
        }

        public Shape(Shape other) {
            m_Vertices = other.Vertices;
        }

        public Shape(Geometry geometry) : this(geometry, 0.0f, Vector2.One) {}

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

        public Shape(Shape baseShape, float rotation, Vector2 scale) {
            double rad = float.DegreesToRadians(-rotation);
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);

            m_Vertices = new Vector2[baseShape.Vertices.Length];

            for (int i = 0; i < baseShape.Vertices.Length; i++) {
                Vector2 scaled = baseShape.Vertices[i] * scale;
                m_Vertices[i] = new Vector2(scaled.X * cos - scaled.Y * sin,
                                            scaled.Y * cos + scaled.X * sin);
            }
        }

        private Vector2[] BuildCircle(float diameter) {
            Vector2[] vertices = new Vector2[2];
            vertices[0] = Vector2.Zero;
            vertices[1] = new Vector2(diameter / 2, 0);

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

        public bool Equals(Shape other)
        {
            return m_Vertices == other.Vertices;
        }
    }
}

