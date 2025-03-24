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
        public Collidable[][] Partition(Collidable[] collidables)
        {
            Collidable[][] partition = new Collidable[1][];

            partition[0] = new Collidable[collidables.Count()];

            for (int i = 0; i < collidables.Length; i++) {
                partition[0][i] = collidables[i];
            }

            return partition;
        }
    }
}
