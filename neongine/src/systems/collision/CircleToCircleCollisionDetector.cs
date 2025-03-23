using System;
using Microsoft.Xna.Framework;

namespace neongine {
    public class CircleToCircleCollisionDetector : ICollisionDetector
    {
        public (Shape.Type, Shape.Type) Shapes => (Shape.Type.Circle, Shape.Type.Circle);

        public bool Collide(Point p1, Collider c1, Point p2, Collider c2, out Collision collision)
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

            collision = new Collision(p1.EntityID, p1, c1, p2.EntityID, p2, c2);
            return true;
        }
    }
}

