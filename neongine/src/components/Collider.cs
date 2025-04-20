using System.Drawing;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Collider : IComponent
    {
        public EntityID EntityID { get; set; }

        [Serialize]
        private Shape m_BaseShape;
        public Shape BaseShape => m_BaseShape;

        [Serialize]
        private float m_Size = 1;
        public float Size => m_Size;

        [Serialize]
        private float m_Rotation = 0.0f;
        public float Rotation = 0.0f;

        [Serialize]
        public bool IsTrigger = false;

        private Shape m_Shape;
        public Shape Shape => m_Shape;
        
        private Bounds m_Bound;
        public Bounds Bound => m_Bound;

        private Shape m_CachedBaseShape;
        private float m_CachedRotation = float.NegativeInfinity;
        private Vector2 m_CachedScale;

        public Collider() : this(new Geometry(GeometryType.Rectangle, 1), 1, 0.0f, false) { }

        public Collider(Shape baseShape, bool isTrigger = false) : this(baseShape, 1, 0.0f, isTrigger) { }
        public Collider(Shape baseShape, float size, bool isTrigger = false) : this(baseShape, size, 0.0f, isTrigger) { }
        public Collider(Shape baseShape, float size, float rotation, bool isTrigger = false)
        {
            m_BaseShape = baseShape;
            m_Size = size;
            m_Rotation = rotation;
            IsTrigger = isTrigger;

            m_Shape = new Shape(m_BaseShape, m_Rotation, Vector2.One);
            m_Bound = new Bounds(m_Shape);
        }

        public Collider(Collider other) : this(other.BaseShape, other.Size, other.Rotation, other.IsTrigger) { }

        public bool UpdateShape(float pointRotation, Vector2 scale) {
            if (m_CachedRotation == (this.Rotation + pointRotation) % 360
                && m_CachedScale == scale
                && m_CachedBaseShape.Equals(this.BaseShape))
                return false;
            
            float targetRotation = (this.Rotation + pointRotation) % 360;

            m_Shape = new Shape(this.BaseShape, targetRotation, scale * this.Size);

            m_CachedRotation = targetRotation;
            m_CachedBaseShape = this.BaseShape;
            m_CachedScale = scale;

            m_Bound.Update(m_Shape);

            return true;
        }

        public IComponent Clone() => new Collider(this);
    }
}
