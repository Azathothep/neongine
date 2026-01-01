using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public interface ICollisionResolver
    {
        public void Resolve(CollisionData[] collisionDatas, EntityID[] entityIDs, Transform[] transforms, Vector2[] positions, Velocity[] velocities, bool[] isStatic, float deltaTime);
    }
}
