using System;
using Microsoft.Xna.Framework;

namespace neongine {
    public class CircleToCircleCollisionDetector : ICollisionDetector
    {
        public (Shape.Type, Shape.Type) Shapes => (Shape.Type.Circle, Shape.Type.Circle);

        public bool Collide(Collidable collidable1, Collidable collidable2, out Collision collision)
        {
            Vector3 difference = collidable1.Point.WorldPosition - collidable2.Point.WorldPosition;
            difference.Z = 0;
            float distanceSqr = difference.LengthSquared();

            float radiuses = collidable1.Collider.Width * collidable1.Point.WorldScale.X + collidable2.Collider.Width * collidable2.Point.WorldScale.X;
            float radiusesSqr = radiuses * radiuses;

            if (distanceSqr > radiusesSqr) {
                collision = null;
                return false;
            }

            collision = new Collision(collidable1, collidable2);
            return true;
        }
    }
}

