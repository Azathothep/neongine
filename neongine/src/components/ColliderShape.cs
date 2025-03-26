using System;
using Microsoft.Xna.Framework;
using neon;

namespace neongine {
    [DoNotSerialize]
    public class ColliderShape : IComponent
    {
        public EntityID EntityID { get; set; }

        public Shape m_Shape;
        public Shape Shape => m_Shape;

        private Geometry m_CachedGeometry;
        private float m_CachedRotation = float.NegativeInfinity;
        private Vector2 m_CachedScale;

        public ColliderShape() {

        }

        public ColliderShape(Collider collider, Point point) {
            ForceUpdate(collider, point);
        }

        public bool Update(Collider collider, Point point) {
            if (m_CachedRotation == collider.Rotation + point.WorldRotation
                && m_CachedScale == point.WorldScale
                && m_CachedGeometry.Equals(collider.Geometry))
                return false;
            
            ForceUpdate(collider, point);
            return true;
        }

        private void ForceUpdate(Collider collider, Point point) {
            float targetRotation = (collider.Rotation + point.WorldRotation) % 360;

            m_Shape = new Shape(collider.Geometry, targetRotation, point.WorldScale * collider.Size);

            m_CachedRotation = targetRotation;
            m_CachedGeometry = collider.Geometry;
            m_CachedScale = point.WorldScale;
        }

        public ColliderShape(ColliderShape other) {
            m_Shape = other.m_Shape;
            m_CachedGeometry = other.m_CachedGeometry;
            m_CachedRotation = other.m_CachedRotation;
            m_CachedScale = other.m_CachedScale;
        }

        public IComponent Clone() => new ColliderShape(this);
    }
}

