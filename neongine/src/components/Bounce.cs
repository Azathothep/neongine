using System;
using neon;

namespace neongine {
    public class Bounce : IComponent
    {
        public EntityID EntityID { get; set; }

        [Serialize]
        public float Value = 1;

        public Bounce() {

        }

        public Bounce(float value) {
            Value = value;
        }

        public Bounce(Bounce other) {
            Value = other.Value;
        }

        public IComponent Clone() => new Bounce(this);
    }
}

