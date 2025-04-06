using neon;

namespace neongine
{
    public class IsStatic : IComponent
    {
        public EntityID EntityID { get; private set; }

        public IComponent Clone() => new IsStatic();
    }
}
