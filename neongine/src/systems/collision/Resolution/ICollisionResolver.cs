using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    /// <summary>
    /// Interface to implement a collision resolution system, used by the <c>CollisionSystem</c>
    /// </summary>
    public interface ICollisionResolver
    {
        /// <summary>
        /// Resolve collisions using the provided <c>CollisionData</c> array.
        /// The <c>CollisionSystem</c> use SOA (Structure of Arrays) approach in storing datas.
        /// The array required as argument respect this structure. Thus, you can view the array indices as IDs : all the elements located at the same index in any array are related to the same entity.
        /// </summary>
        public void Resolve(CollisionData[] collisionDatas, EntityID[] entityIDs, Transform[] transforms, Vector2[] positions, Velocity[] velocities, bool[] isStatic, float deltaTime);
    }
}
