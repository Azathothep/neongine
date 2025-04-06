using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public class NoCollisionResolver : ICollisionResolver
    {
        public void Resolve(Collision[] collisionDatas, (int, int)[] indices, Point[] points, ColliderShape[] shapes, Velocity[] velocities, bool[] isStatic) { }
    }
}
