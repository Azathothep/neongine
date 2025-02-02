using Microsoft.Xna.Framework;
using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public class Velocity : IComponent
    {
        public EntityID EntityID { get; private set; }

        [Serialize]
        public Vector3 velocity;

        public Velocity()
        {

        }

        public Velocity(Vector3 direction)
        {
            this.velocity = direction;
        }

        public Velocity(Velocity velocity)
        {
            this.velocity = velocity.velocity;
        }

        public IComponent Clone()
        {
            return new Velocity(this);
        }
    }
}
