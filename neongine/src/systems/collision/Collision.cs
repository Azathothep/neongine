using System.Numerics;
using neon;

namespace neongine
{
    public class Collision
    {
        private EntityID m_ID1;
        public EntityID ID1 => m_ID1;

        private EntityID m_ID2;
        public EntityID ID2 => m_ID2;

        public Collision() {
        }
    }
}
