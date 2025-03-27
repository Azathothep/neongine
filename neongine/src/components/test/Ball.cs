using System;
using neon;

namespace neongine {
    public class Ball : IComponent
    {
        public EntityID EntityID { get; private set; }

        public IComponent Clone() => new Ball();
    }
}

