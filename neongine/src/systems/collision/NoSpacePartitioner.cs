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
        public EntityID[][] Partition(IEnumerable<(EntityID, Point, Collider)> m_Content)
        {
            EntityID[][] partition = new EntityID[1][];

            partition[0] = new EntityID[m_Content.Count()];

            int i = 0;
            foreach ((EntityID eid, Point _, Collider _) in m_Content) {
                partition[0][i] = eid;
                i++;
            }

            return partition;
        }
    }
}
