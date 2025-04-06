using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public interface ICollisionResolver
    {
        public void Resolve(Collision[] collisionDatas, (int, int)[] indices, Point[] points, ColliderShape[] shapes, Velocity[] velocities, bool[] isStatic);
    }
}
