using System.Drawing;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Collider : IComponent
    {
        public EntityID EntityID { get; set; }

        [Serialize]
        private Geometry m_Geometry;
        public Geometry Geometry => m_Geometry;

        [Serialize]
        private float m_Size = 1;
        public float Size => m_Size;

        [Serialize]
        private float m_Rotation = 0.0f;
        public float Rotation = 0.0f;

        public float Width => m_Geometry.Width * m_Size;
        public float Height => m_Geometry.Height * m_Size;

        [Serialize]
        public bool IsTrigger = false;

        public Collider() : this(new Geometry(GeometryType.Rectangle, 1), 1, 0.0f, false) {
        }

        public Collider(Geometry geometry) : this(geometry, 1, 0.0f, false) {
        }

        public Collider(Geometry geometry, int size, bool isTrigger = false) : this(geometry, size, 0.0f, isTrigger) {
        }

        public Collider(Geometry geometry, int size, float rotation, bool isTrigger = false)
        {
            m_Geometry = geometry;
            m_Size = size;
            m_Rotation = rotation;
            IsTrigger = isTrigger;
        }

        public Collider(Collider other)
        {
            m_Geometry = other.m_Geometry;
            m_Size = other.m_Size;
            m_Rotation = other.m_Rotation;
            IsTrigger = other.IsTrigger;
        }

        public IComponent Clone() => new Collider(this);
    }
}
