using System;
using System.Diagnostics;
using System.Numerics;
using neon;

namespace neongine {

    [DoNotSerialize]
    public class ColliderBounds : IComponent
    {
        public EntityID EntityID { get; set; }

        private Bounds m_Bounds;
        public Bounds Bounds => m_Bounds;

        public ColliderBounds() {

        }

        public ColliderBounds(Shape shape) {
            Update(shape);
        }

        public void Update(Shape shape) {
            m_Bounds = new Bounds(shape);
        }

        public ColliderBounds(ColliderBounds other) {
            m_Bounds = other.m_Bounds;
        }

        public IComponent Clone() => new ColliderBounds(this);
    }
}

