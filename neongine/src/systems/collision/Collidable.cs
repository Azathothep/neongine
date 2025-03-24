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

        private Bound m_Bound;
        public Bound Bound => m_Bound;

        public Collidable(EntityID entityID, Point point, Collider collider, Bound bound) {
            m_EntityID = entityID;
            m_Point = point;
            m_Collider = collider;
            m_Bound = bound;
        }

    }
}

