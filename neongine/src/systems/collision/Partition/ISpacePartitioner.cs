using Microsoft.Xna.Framework;
using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    /// <summary>
    /// Interface for implementing a Space Partitioner used by the Collision System.
    /// </summary>
    public interface ISpacePartitioner
    {
        /// <summary>
        /// Partition the space between the provided entities and return an enumerable of all the entity pairs that have to be checked for collision detection
        /// The <c>CollisionSystem</c> use SOA (Structure of Arrays) approach in storing datas.
        /// The array required as argument respect this structure. Thus, you can view the array indices as IDs : all the elements located at the same index in any array are related to the same entity.
        /// </summary>
        public IEnumerable<(EntityID, EntityID)> Partition(EntityID[] ids, Vector2[] positions, Bounds[] bounds);
    }
}
