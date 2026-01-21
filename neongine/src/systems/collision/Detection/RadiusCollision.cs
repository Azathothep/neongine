using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace neongine {
    /// <summary>
    /// Utility functions for detecting collisions between circles
    /// </summary>
    public static class RadiusCollision
    {
        /// <summary>
        /// Returns true if the circle of radius <c>radius1</c> at position <c>p1</c> is overlapping with the circle of radius <c>radius2</c> at position <c>p2</c>.
        /// </summary>
        public static bool Collide(Vector2 p1, float radius1, Vector2 p2, float radius2)
        {
            Vector2 difference = p1 - p2;
            float distanceSqr = difference.LengthSquared();

            float radiuses = radius1 + radius2;
            float radiusesSqr = radiuses * radiuses;

            if (distanceSqr > radiusesSqr) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the circle of radius <c>radius1</c> at position <c>p1</c> is overlapping with the circle of radius <c>radius2</c> at position <c>p2</c>.
        /// If applicable, also fills overlap datas in a <c>Collision</c> object.
        /// </summary>
        public static bool Collide(Vector2 p1, float radius1, Vector2 p2, float radius2, out Collision collision)
        {
            Vector2 difference = p1 - p2;
            float distance = difference.Length();

            float radiuses = radius1 + radius2;

            if (distance > radiuses) {
                collision = null;
                return false;
            }

            difference.Normalize();
            Penetration penetrationOnEntity1 = new Penetration(difference, distance);
            Penetration penetrationOnEntity2 = new Penetration(- difference, distance);

            collision = new Collision() {
                Penetration = [ penetrationOnEntity1, penetrationOnEntity2 ]
            };

            return true;
        }
    }
}

