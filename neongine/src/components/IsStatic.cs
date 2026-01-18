using neon;

namespace neongine
{
    /// <summary>
    /// Flag component to indicate static (non-moving) entities, for optimizing collision detection.
    /// </summary>
    public class IsStatic : Component
    {
        public override Component Clone() => new IsStatic();
    }
}
