namespace neongine
{
    public interface IGame
    {
        public int WindowWidth {get; }
        public int WindowHeight {get; }

        public void Load();
    }
}