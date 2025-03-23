using System.Numerics;
using neon;

namespace neongine
{
    public class Collision
    {
        private ((EntityID id, Point point, Collider collider), (EntityID id, Point point, Collider collider)) m_Datas;
        public ((EntityID id, Point point, Collider collider), (EntityID id, Point point, Collider collider)) Datas => m_Datas;

        public Collision(EntityID id1, Point p1, Collider c1, EntityID id2, Point p2, Collider c2) {
            m_Datas = ((id1, p1, c1), (id2, p2, c2));
        }
    }
}
