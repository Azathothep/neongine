using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace neongine {
    [Serializable]
    public struct Rect {
        [Serialize]
        public float X;

        [Serialize]
        public float Y;

        [Serialize]
        public float Width;

        [Serialize]
        public float Height;

        public Rect() {

        }

        public Rect(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }
}
