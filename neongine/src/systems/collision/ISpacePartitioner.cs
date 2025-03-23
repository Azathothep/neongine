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
        public  (EntityID, Point, Collider)[][] Partition(IEnumerable<(EntityID, Point, Collider)> m_Content);
    }
}
