using System.Collections.Generic;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public interface ICollisionDetector
    {
        public void Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds, out CollisionData[] collisionData);
        public (EntityID, EntityID)[] Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds);
    }
}