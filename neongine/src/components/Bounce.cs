using System;
using neon;

namespace neongine {
    public class Bounce : Component
    {
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

        public override Component Clone() => new Bounce(this);
    }
}

