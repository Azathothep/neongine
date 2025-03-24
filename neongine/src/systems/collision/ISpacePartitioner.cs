using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public interface ISpacePartitioner
    {
        public  (EntityID, Point, Collider, Bound)[][] Partition((EntityID, Point, Collider)[] content, Bound[] bounds);
    }
}
