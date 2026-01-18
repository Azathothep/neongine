using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    /// <summary>
    /// Adds a rotation velocity to the entity
    /// </summary>
    public class AngleVelocity : Component
    {
        [Serialize]
        private float m_Value = 1.0f;

        /// <summary>
        /// The speed, in degrees per seconds, at which the entity rotates
        /// </summary>
        public float Value {
            get => m_Value;
            set => m_Value = value;
        }

        public AngleVelocity() {}

        public AngleVelocity(float speed)
        {
            m_Value = speed;
        }

        public AngleVelocity(AngleVelocity other) : this(other.Value) { }

        public override Component Clone()
        {
            return new AngleVelocity(this);
        }
    }
}
