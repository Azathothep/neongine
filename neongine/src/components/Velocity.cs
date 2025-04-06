using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Velocity : IComponent
    {
        public EntityID EntityID { get; private set; }

        [Serialize]
        public Vector3 Value;

        public Velocity()
        {

        }

        public Velocity(Vector3 value)
        {
            this.Value = value;
        }

        public Velocity(Velocity velocity)
        {
            this.Value = velocity.Value;
        }

        public IComponent Clone()
        {
            return new Velocity(this);
        }
    }
}
