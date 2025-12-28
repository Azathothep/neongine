using System;
using neon;

namespace neongine.editor
{
    public interface IEditorUpdateSystem : IUpdateSystem
    {
        public bool ActiveInPlayMode {get; }
    }
}

