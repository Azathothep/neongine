using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Velocity : IComponent
    {
        public EntityID EntityID { get; private set; }

        [Serialize]
        public Vector3 Direction;

        public Velocity()
        {

        }

        public Velocity(Vector3 direction)
        {
            this.Direction = direction;
        }

        public Velocity(Velocity velocity)
        {
            this.Direction = velocity.Direction;
        }

        public IComponent Clone()
        {
            return new Velocity(this);
        }
    }
}
