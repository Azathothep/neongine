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

        public Collision(EntityID id1, EntityID id2) {
            m_ID1 = id1;
            m_ID2 = id2;
        }
    }
}
