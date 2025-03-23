using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public interface ICollisionProcessor
    {
        public bool Collide(Point p1, Collider c1, Point p2, Collider c2, out Collision collision);
    }
}