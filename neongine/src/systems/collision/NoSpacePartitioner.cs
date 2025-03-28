using neon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public class NoSpacePartitioner : ISpacePartitioner
    {
        public IEnumerable<(int, int)> Partition(Point[] points, ColliderBounds[] colliderBounds)
        {
            List<(int, int)> partition = new();

            for (int i = 0; i < points.Length - 1; i++) {
                for (int j = i + 1; j < points.Length; j++) {
                    partition.Add((i, j));
                }
            }

            return partition;
        }
    }
}
