using System;

namespace neongine {
    public class BaseCollisionResolver : ICollisionResolver
    {
        public void Resolve(Collision[] collisionDatas, (int, int)[] indices, Point[] points, ColliderShape[] shapes, Velocity[] velocities, bool[] isStatic)
        {
            // Add normal collision & penetration data

            // If both (static or have no velocity) -> don't solve

            // Case 1 : it they have no Bounce component
                // If only 1 has velocity (& no static, of couse) -> update it linearly
                // If both have velocity -> find where to relocate them based on their velocity (& velocity "speed")
            // Case 2 : one of them has a bounce component
                // If the other one is static : update the first according to its velocity
                // If the other one is not static : ???
                // If the other one also has a bounce component : ???
            
            // Make sure to calculate all the results before applying them : we still have to check for multiple collisions happening at the same time for the same object
        }
    }
}

