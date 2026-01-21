using System.Collections.Generic;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    /// <summary>
    /// Interface for implementing a Collision Detector used by the <c>CollisionSystem</c>.
    /// </summary>
    public interface ICollisionDetector
    {
        /// <summary>
        /// Detect collisions and fill a <c>CollisionData</c> array with the validated collisions.
        /// The first argument gives all the pairs you need to check in detecting collisions. This was filled by a <c>ISpacePartitioner</c> previously called by the <c>CollisionSystem</c>.
        /// The <c>CollisionSystem</c> use SOA (Structure of Arrays) approach in storing datas.
        /// The array required as argument respect this structure. Thus, you can view the array indices as IDs : all the elements located at the same index in any array are related to the same entity.
        /// </summary>
        public void Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds, out CollisionData[] collisionData);
        
        /// <summary>
        /// Detect collisions and fill an array of tuples with the validated collisions.
        /// The first argument gives all the pairs you need to check in detecting collisions. This was filled by a <c>ISpacePartitioner</c> previously called by the <c>CollisionSystem</c>.
        /// The <c>CollisionSystem</c> use SOA (Structure of Arrays) approach in storing datas.
        /// The array required as argument respect this structure. Thus, you can view the array indices as IDs : all the elements located at the same index in any array are related to the same entity.
        /// </summary>
        public (EntityID, EntityID)[] Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds);
    }
}