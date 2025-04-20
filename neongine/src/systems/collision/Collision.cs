using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public struct Penetration {
        public Vector2 Axis;
        public float Length;
        public Penetration(Vector2 axis, float length) {
            Axis = axis;
            Length = length;
        }
    }
    public class Collision
    {
        public Penetration[] Penetration;

        public Collision() {
            
        }
    }
}
