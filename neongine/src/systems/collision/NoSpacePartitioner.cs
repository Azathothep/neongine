using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace neongine
{
    public class NoSpacePartitioner : ISpacePartitioner
    {
        public IEnumerable<(int, int)> Partition(Vector3[] positions, ColliderBounds[] colliderBounds)
        {
            List<(int, int)> partition = new();

            for (int i = 0; i < positions.Length - 1; i++) {
                for (int j = i + 1; j < positions.Length; j++) {
                    partition.Add((i, j));
                }
            }

            return partition;
        }
    }
}
