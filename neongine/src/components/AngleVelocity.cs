using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public class AngleVelocity : IComponent
    {
        public EntityID EntityID { get ; private set ; }

        private float m_Value;
        public float Value => m_Value;

        public AngleVelocity(float speed)
        {
            m_Value = speed;
        }

        public AngleVelocity(AngleVelocity other) : this(other.Value) { }

        public IComponent Clone()
        {
            return new AngleVelocity(this);
        }
    }
}
