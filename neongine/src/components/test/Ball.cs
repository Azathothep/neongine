using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using neon;

namespace neongine {
    [DoNotSerialize]
    public class Ball : IComponent
    {
        public EntityID EntityID { get; private set; }

        public IComponent Clone() => new Ball();
    }
}

