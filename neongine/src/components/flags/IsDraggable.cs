using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neon
{
    public class IsDraggable : Component
    {
        public override Component Clone()
        {
            return new IsDraggable();
        }
    }
}
