using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    /// <summary>
    /// Implements <c>ICollisionResolver</c> but do not provide any collision resolution.
    /// </summary>
    public class NoCollisionResolver : ICollisionResolver
    {
        public void Resolve(CollisionData[] collisionDatas, EntityID[] entityIDs, Transform[] transforms, Vector2[] positions, Velocity[] velocities, bool[] isStatic, float deltaTime) { }
    }
}
