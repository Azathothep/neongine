using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Collision
    {
        public struct PenetrationData {
            public Vector2 Axis;
            public float Length;
            public PenetrationData(Vector2 axis, float length) {
                Axis = axis;
                Length = length;
            }
        }

        public PenetrationData[] PenetrationOnEntity1;
        public PenetrationData[] PenetrationOnEntity2;

        public Collision() {
            
        }
    }
}
