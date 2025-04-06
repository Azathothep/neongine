using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace neongine
{
    public interface ICollisionProcessor
    {
        public void GetCollisions(IEnumerable<(int, int)> partition, Vector3[] positions, Collider[] colliders, ColliderShape[] colliderShapes, ColliderBounds[] colliderBounds, out ((int, int)[], Collision[]) collisions, out (int, int)[] triggers);
    }
}