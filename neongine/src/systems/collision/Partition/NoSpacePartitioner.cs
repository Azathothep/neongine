using Microsoft.Xna.Framework;
using neon;
using System.Collections.Generic;

namespace neongine
{
    public class NoSpacePartitioner : ISpacePartitioner
    {
        public IEnumerable<(EntityID, EntityID)> Partition(EntityID[] ids, Vector2[] positions, Bounds[] bounds)
        {
            List<(EntityID, EntityID)> partition = new();

            for (int i = 0; i < positions.Length - 1; i++) {
                for (int j = i + 1; j < positions.Length; j++) {
                    partition.Add((ids[i], ids[j]));
                }
            }

            return partition;
        }
    }
}
