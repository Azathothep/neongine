using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public class NoSpacePartitioner : ISpacePartitioner
    {
        public (EntityID, Point, Collider, Bound)[][] Partition((EntityID, Point, Collider)[] content, Bound[] bounds)
        {
            (EntityID, Point, Collider, Bound)[][] partition = new (EntityID, Point, Collider, Bound)[1][];

            partition[0] = new (EntityID, Point, Collider, Bound)[content.Count()];

            for (int i = 0; i < content.Length; i++) {
                (EntityID id, Point p, Collider c) = content[i];
                partition[0][i] = (id, p, c, bounds[i]);
            }

            return partition;
        }
    }
}
