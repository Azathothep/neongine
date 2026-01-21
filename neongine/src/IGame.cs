namespace neongine
{
    /// <summary>
    /// This class is the entry point for your application. You can load editor-only content and game-only content by implementing the related methods.
    /// </summary>
    public interface IGame
    {
        /// <summary>
        /// Game window Width in pixels (standard: 800)
        /// </summary>
        public int WindowWidth {get; }

        /// <summary>
        /// Game window Height in pixels (standard: 480)
        /// </summary>
        public int WindowHeight {get; }

        /// <summary>
        /// Load editor-only code. Put all your editor-related content inside. This won't be called in the built application.
        /// </summary>
        public void EditorLoad();

        /// <summary>
        ///Load game-only code. Put all your game-related content inside.
        /// </summary>
        public void GameLoad();
    }
}