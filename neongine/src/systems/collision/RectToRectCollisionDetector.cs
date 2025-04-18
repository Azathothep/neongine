using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace neongine {
    public class RectToRectCollisionDetector : ICollisionDetector
    {
        public (GeometryType, GeometryType) Shapes => (GeometryType.Rectangle, GeometryType.Rectangle);

        public bool Collide(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2)
        {
            return !HasSeparatingAxis(p1, s1, p2, s2);
        }

        public bool Collide(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2, out Collision collision)
        {
            collision = new Collision();
            return !HasSeparatingAxis(p1, s1, p2, s2);
        }

        private bool HasSeparatingAxis(Vector3 p1, Shape s1, Vector3 p2, Shape s2) {
            Vector2[] normals = [.. GetNormals(s1), .. GetNormals(s2)];

            for (int i = 0; i < normals.Length; i++) {
                (float min1, float max1) = GetMinMax(p1, s1, normals[i]);
                (float min2, float max2) = GetMinMax(p2, s2, normals[i]);

                if ((min1 < min2 && max1 < min2)
                    || (min2 < min1 && max2 < min1)) {
                    return true;
                }
            }

            return false;
        }

        private (float, float) GetMinMax(Vector3 position, Shape shape, Vector2 axis) {
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int i = 0; i < shape.Vertices.Length; i++) {
                Vector2 verticePosition = new Vector2(position.X, position.Y) + shape.Vertices[i];
                float length  = Vector2.Dot(verticePosition, axis);
                if (length < minValue)
                    minValue = length;
                
                if (length > maxValue)
                    maxValue = length;
            }

            return (minValue, maxValue);
        }

        private Vector2[] GetNormals(Shape shape) {
            int length = shape.Vertices.Length;

            Vector2[] normals = new Vector2[length];

            for (int i = 0; i < shape.Vertices.Length - 1; i++) {
                Vector2 edge = shape.Vertices[i + 1] - shape.Vertices[i];
                normals[i] = new Vector2(-edge.Y, edge.X);
                normals[i].Normalize();
            }

            Vector2 lastEdge = shape.Vertices[length - 1] - shape.Vertices[0];
            normals[length - 1] = new Vector2(-lastEdge.Y, lastEdge.X);

            return normals;
        }
    }
}

