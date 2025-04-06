using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace neongine {
    public class CircleToCircleCollisionDetector : ICollisionDetector
    {
        public (GeometryType, GeometryType) Shapes => (GeometryType.Circle, GeometryType.Circle);

        public bool Collide(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2)
        {
            Vector3 difference = p1 - p2;
            difference.Z = 0;
            float distanceSqr = difference.LengthSquared();

            float radiuses = c1.Size * s1.Vertices[1].X + c2.Size * s2.Vertices[1].X;
            float radiusesSqr = radiuses * radiuses;

            if (distanceSqr > radiusesSqr) {
                return false;
            }
            return true;
        }

        public bool Collide(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2, out Collision collision)
        {
            Vector2 difference = new Vector2(p1.X, p1.Y) - new Vector2(p2.X, p2.Y);
            float distance = difference.Length();

            float radiuses = c1.Size * s1.Vertices[1].X + c2.Size * s2.Vertices[1].X;

            if (distance > radiuses) {
                collision = null;
                return false;
            }

            difference.Normalize();
            Collision.PenetrationData[] penetrationOnEntity1 = [ new Collision.PenetrationData(difference, distance) ];
            Collision.PenetrationData[] penetrationOnEntity2 = [ new Collision.PenetrationData(- difference, distance) ];

            collision = new Collision() {
                PenetrationOnEntity1 = penetrationOnEntity1,
                PenetrationOnEntity2 = penetrationOnEntity2
            };
            return true;
        }
    }
}

