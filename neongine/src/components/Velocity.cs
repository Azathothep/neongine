using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Velocity : Component
    {
        [Serialize]
        public Vector2 Value;

        public Velocity()
        {

        }

        public Velocity(Vector2 value)
        {
            this.Value = value;
        }

        public Velocity(Velocity velocity)
        {
            this.Value = velocity.Value;
        }

        public override Component Clone()
        {
            return new Velocity(this);
        }
    }
}
