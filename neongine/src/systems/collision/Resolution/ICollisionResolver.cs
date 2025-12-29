using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using neon;

namespace neongine
{
    public interface ICollisionResolver
    {
        public void Resolve(CollisionData[] collisionDatas, EntityID[] entityIDs, Velocity[] velocities, bool[] isStatic, float deltaTime);
    }
}
