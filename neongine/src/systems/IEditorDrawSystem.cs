using System;
using neon;

namespace neongine.editor
{
    public interface IEditorDrawSystem : IDrawSystem
    {
        public bool ActiveInPlayMode {get; }
    }
}

