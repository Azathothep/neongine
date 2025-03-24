using System.Numerics;
using neon;

namespace neongine
{
    public class Collision
    {
        private Collidable m_Collidable1;
        private Collidable m_Collidable2;

        public Collidable Collidable1 => m_Collidable1;
        public Collidable Collidable2 => m_Collidable2;

        public Collision(Collidable collidable1, Collidable collidable2) {
            m_Collidable1 = collidable1;
            m_Collidable2 = collidable2;
        }
    }
}
