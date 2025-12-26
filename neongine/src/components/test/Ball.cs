using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using neon;

namespace neongine {
    [DoNotSerialize]
    public class Ball : Component
    {
        public override Component Clone() => new Ball();
    }
}

