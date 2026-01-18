using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neon
{
    /// <summary>
    /// Editor-only flag component to indicate an entity as non-draggable by the EditorDragSystem
    /// </summary>
    public class NotDraggable : Component
    {
        public override Component Clone()
        {
            return new NotDraggable();
        }
    }
}
