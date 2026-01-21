using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace neongine {
    /// <summary>
    /// Utility for using the Separating Axis Theorem
    /// </summary>
    public static class SeparatingAxisCollision
    {
        private static float PRECISION = 0.01f;

        public static bool PolygonsCollide(Vector2 p1, Shape s1, Vector2 p2, Shape s2)
        {
            return !HasSeparatingAxis(p1, s1, p2, s2);
        }

        public static bool PolygonsCollide(Vector2 p1, Shape s1, Vector2 p2, Shape s2, out Collision collision)
        {
            return !HasSeparatingAxis(p1, s1, p2, s2, out collision);
        }

        public static bool CircleCollide(Vector2 polygonPosition, Shape polygonShape, Vector2 circlePosition, float circleRadius) {
            return !HasSeparatingAxis(polygonPosition, polygonShape, circlePosition, circleRadius);
        }

        public static bool CircleCollide(Vector2 polygonPosition, Shape polygonShape, Vector2 circlePosition, float circleRadius, out Collision collision) {
            return !HasSeparatingAxis(polygonPosition, polygonShape, circlePosition, circleRadius, out collision);
        }

        private static bool HasSeparatingAxis(Vector2 p1, Shape s1, Vector2 p2, Shape s2) {
            Vector2[] normals = GetNormals(s1, s2);
            
            for (int i = 0; i < normals.Length; i++) {
                (float min1, float max1) = GetMinMax(p1, s1, normals[i]);
                (float min2, float max2) = GetMinMax(p2, s2, normals[i]);

                float difference;
                if (min1 < min2)
                    difference = max1 - min2;
                else
                    difference = max2 - min1;
                    
                if (difference <= PRECISION) {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSeparatingAxis(Vector2 p1, Shape s1, Vector2 p2, Shape s2, out Collision collision) {
            Vector2[] normals = GetNormals(s1, s2);

            Penetration[] penetration = new Penetration[normals.Length];

            for (int i = 0; i < normals.Length; i++) {
                (float min1, float max1) = GetMinMax(p1, s1, normals[i]);
                (float min2, float max2) = GetMinMax(p2, s2, normals[i]);

                float difference;
                if (min1 < min2)
                    difference = max1 - min2;
                else
                    difference = max2 - min1;
                    
                if (difference <= PRECISION) {
                    collision = null;
                    return true;
                }

                penetration[i] = new Penetration() {
                    Axis = normals[i],
                    Length = Math.Abs(difference)
                };
            }

            collision = new Collision() {
                Penetration = penetration
            };

            return false;
        }

        private static bool HasSeparatingAxis(Vector2 polygonPosition, Shape polygonShape, Vector2 circlePosition, float circleRadius) {
            Vector2 normal = Vector2.Normalize(circlePosition - polygonPosition);
            
            (float min1, float max1) = GetMinMax(polygonPosition, polygonShape, normal);

            float circlePosOnNormal = Vector2.Dot(circlePosition, normal);
            (float min2, float max2) = (circlePosOnNormal - circleRadius, circlePosOnNormal + circleRadius);

            float difference;
            if (min1 < min2)
                difference = max1 - min2;
            else
                difference = max2 - min1;
                
            if (difference <= PRECISION) {
                return true;
            }

            return false;
        }

        private static bool HasSeparatingAxis(Vector2 polygonPosition, Shape polygonShape, Vector2 circlePosition, float circleRadius, out Collision collision) {
            Vector2 normal = Vector2.Normalize(circlePosition - polygonPosition);
            
            (float min1, float max1) = GetMinMax(polygonPosition, polygonShape, normal);
            
            float circlePosOnNormal = Vector2.Dot(circlePosition, normal);
            (float min2, float max2) = (circlePosOnNormal - circleRadius, circlePosOnNormal + circleRadius);

            float difference;
            if (min1 < min2)
                difference = max1 - min2;
            else
                difference = max2 - min1;
                
            if (difference <= PRECISION) {
                collision = null;
                return true;
            }

            collision = new Collision();
            collision.Penetration = [ new Penetration()
                                        {
                                            Axis = normal,
                                            Length = Math.Abs(difference)
                                        }
                                    ];

            return false;
        }

        /// <summary>
        /// Get the min and max values of the projected shape along the provided axis
        /// </summary>
        private static (float, float) GetMinMax(Vector2 position, Shape shape, Vector2 axis) {
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

        /// <summary>
        /// Get a collection of both shape's normals
        /// </summary>
        private static Vector2[] GetNormals(Shape shape1, Shape shape2) {
            Vector2[] normals1 = GetNormals(shape1);
            Vector2[] normals2 = GetNormals(shape2);

            List<Vector2> normals = normals1.ToList();
            foreach (var n in normals2) {
                if (normals.Contains(n) || normals.Contains(-n))
                    continue;

                normals.Add(n);
            }

            return normals.ToArray();
        }

        /// <summary>
        /// Get a collection of the shape's normals
        /// </summary>
        private static Vector2[] GetNormals(Shape shape) {
            int length = shape.Vertices.Length;

            Vector2[] normals = new Vector2[length];

            for (int i = 0; i < shape.Vertices.Length - 1; i++) {
                Vector2 edge = shape.Vertices[i + 1] - shape.Vertices[i];
                normals[i] = new Vector2(-edge.Y, edge.X);
                normals[i].Normalize();
            }

            Vector2 lastEdge = shape.Vertices[0] - shape.Vertices[length - 1];
            normals[length - 1] = new Vector2(-lastEdge.Y, lastEdge.X);
            normals[length - 1].Normalize();

            return normals;
        }
    }
}

