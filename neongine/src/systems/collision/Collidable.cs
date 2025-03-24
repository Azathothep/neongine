using System;
using neon;

namespace neongine {
    public struct Collidable
    {
        private EntityID m_EntityID;
        public EntityID EntityID => m_EntityID;

        private Point m_Point;
        public Point Point => m_Point;

        private Collider m_Collider;
        public Collider Collider => m_Collider;

        public Collidable(EntityID entityID, Point point, Collider collider) {
            m_EntityID = entityID;
            m_Point = point;
            m_Collider = collider;
        }

    }
}

