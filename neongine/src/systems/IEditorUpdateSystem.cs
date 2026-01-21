using System;
using neon;

namespace neongine.editor
{
    /// <summary>
    /// Editor-only update system. It is disabled when publishing the application.
    /// </summary>
    public interface IEditorUpdateSystem : IUpdateSystem
    {
        /// <summary>
        /// If true, the system will stay active when entering Play mode when in editor
        /// </summary>
        public bool ActiveInPlayMode {get; }
    }
}

