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
        public (Collidable[][], Bound[][]) Partition(Collidable[] collidables, Bound[] bounds)
        {
            Collidable[][] partition = new Collidable[1][];

            partition[0] = new Collidable[collidables.Count()];

            Bound[][] partitionedBounds = new Bound[1][];
            partitionedBounds[0] = new Bound[collidables.Count()];

            for (int i = 0; i < collidables.Length; i++) {
                partition[0][i] = collidables[i];
                partitionedBounds[0][i] = Bound.Get(collidables[i].Collider.Shape, collidables[i].Point.WorldPosition2D, collidables[i].Collider.Size * collidables[i].Point.WorldScale, collidables[i].Point.WorldRotation);
            }

            return (partition, partitionedBounds);
        }
    }
}
