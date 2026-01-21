using Microsoft.Xna.Framework;
using neon;
using System.Collections.Generic;

namespace neongine
{
    /// <summary>
    /// Implements ISpacePartitioner but don't partition any space.
    /// </summary>
    public class NoSpacePartitioner : ISpacePartitioner
    {
        /// <summary>
        /// Return all the pairs that can be made between all entities.
        /// The <c>CollisionSystem</c> use SOA (Structure of Arrays) approach in storing datas.
        /// The array required as argument respect this structure. Thus, you can view the array indices as IDs : all the elements located at the same index in any array are related to the same entity.
        /// </summary>
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
