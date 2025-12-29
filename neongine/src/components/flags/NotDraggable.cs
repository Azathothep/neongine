using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neon
{
    public class NotDraggable : Component
    {
        public override Component Clone()
        {
            return new NotDraggable();
        }
    }
}
