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
        public IEnumerable<Collision> GetCollisions(Collidable[] collidables, Bound[] bounds);
    }
}