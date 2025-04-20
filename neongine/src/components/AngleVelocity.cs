﻿using neon;
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

        [Serialize]
        private float m_Value = 1.0f;
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

        public IComponent Clone()
        {
            return new AngleVelocity(this);
        }
    }
}
