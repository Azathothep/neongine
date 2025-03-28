using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using neon;

namespace neongine
{
    public interface ICollisionProcessor
    {
        public void GetCollisions(IEnumerable<(int, int)> partition, Point[] points, Collider[] colliders, ColliderShape[] colliderShapes, ColliderBounds[] colliderBounds, out ((int, int)[], Collision[]) collisions, out (int, int)[] triggers);
    }
}