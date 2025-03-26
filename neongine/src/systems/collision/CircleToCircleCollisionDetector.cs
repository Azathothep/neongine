using System;
using Microsoft.Xna.Framework;

namespace neongine {
    public class CircleToCircleCollisionDetector : ICollisionDetector
    {
        public (GeometryType, GeometryType) Shapes => (GeometryType.Circle, GeometryType.Circle);

        public bool Collide(Point p1, Collider c1, Shape s1, Point p2, Collider c2, Shape s2)
        {
            Vector3 difference = p1.WorldPosition - p2.WorldPosition;
            difference.Z = 0;
            float distanceSqr = difference.LengthSquared();

            float radiuses = c1.Width * p1.WorldScale.X + c2.Width * p2.WorldScale.X;
            float radiusesSqr = radiuses * radiuses;

            if (distanceSqr > radiusesSqr) {
                return false;
            }
            return true;
        }

        public bool Collide(Point p1, Collider c1, Shape s1, Point p2, Collider c2, Shape s2, out Collision collision)
        {
            Vector3 difference = p1.WorldPosition - p2.WorldPosition;
            difference.Z = 0;
            float distanceSqr = difference.LengthSquared();

            float radiuses = c1.Width * p1.WorldScale.X + c2.Width * p2.WorldScale.X;
            float radiusesSqr = radiuses * radiuses;

            if (distanceSqr > radiusesSqr) {
                collision = null;
                return false;
            }

            collision = new Collision();
            return true;
        }
    }
}

