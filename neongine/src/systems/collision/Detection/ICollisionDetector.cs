using System.Collections.Generic;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public interface ICollisionDetector
    {
        public CollisionDetectionData Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] entityIDs, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds);
    }
}