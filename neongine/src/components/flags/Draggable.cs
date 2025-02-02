using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neon
{
    public class Draggable : IComponent
    {
        public EntityID EntityID { get; set; }

        public IComponent Clone()
        {
            return new Draggable();
        }
    }
}
