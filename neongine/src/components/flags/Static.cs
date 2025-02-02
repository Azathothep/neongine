using neon;

namespace neongine
{
    public class Static : IComponent
    {
        public EntityID EntityID { get; private set; }

        public IComponent Clone() => new Static();
    }
}
