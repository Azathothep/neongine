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
        public int[][] Partition(Point[] points, ColliderBounds[] colliderBounds)
        {
            int[][] partition = new int[1][];

            partition[0] = new int[points.Count()];

            for (int i = 0; i < points.Length; i++) {
                partition[0][i] = i;
            }

            return partition;
        }
    }
}
