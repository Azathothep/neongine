using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    /// <summary>
    /// Adds a position velocity to the entity
    /// </summary>
    public class Velocity : Component
    {
        /// <summary>
        /// The direction and strength of the force applied, in units per seconds
        /// </summary>
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
