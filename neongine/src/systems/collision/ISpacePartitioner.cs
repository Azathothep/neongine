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
        public IEnumerable<(int, int)> Partition(Point[] points, ColliderBounds[] colliderBounds);
    }
}
