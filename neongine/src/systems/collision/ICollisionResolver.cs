using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public interface ICollisionResolver
    {
        public void Resolve(Collision[] collisions);
    }
}
