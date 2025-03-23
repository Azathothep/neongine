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
        public (EntityID, Point, Collider)[][] Partition(IEnumerable<(EntityID, Point, Collider)> m_Content)
        {
            (EntityID, Point, Collider)[][] partition = new (EntityID, Point, Collider)[1][];

            partition[0] = new (EntityID, Point, Collider)[m_Content.Count()];

            int i = 0;
            foreach ((EntityID id, Point p, Collider c) in m_Content) {
                partition[0][i] = (id, p, c);
                i++;
            }

            return partition;
        }
    }
}
