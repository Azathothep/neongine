using System.Drawing;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    /// <summary>
    /// Add the possibility to react to collisions and triggers
    /// </summary>
    public class Collider : Component
    {
        [Serialize]
        private Shape m_BaseShape;

        /// <summary>
        /// The base shape of the collider (doesn't update from rotation nor scale)
        /// </summary>
        public Shape BaseShape => m_BaseShape;

        [Serialize]
        private float m_Size = 1;

        /// <summary>
        /// The collider's shape size
        /// </summary>
        public float Size => m_Size;

        [Serialize]
        private float m_Rotation = 0.0f;
        /// <summary>
        /// The collider's shape rotation
        /// </summary>
        public float Rotation = 0.0f;

        /// <summary>
        /// Does the collider react to collisions or overlaps ?
        /// </summary>
        [Serialize]
        public bool IsTrigger = false;

        private Shape m_Shape;
        /// <summary>
        /// The shape of the collider, updating with the entitie's rotation, the entitie's scale, the collider's rotation and the collider's size 
        /// </summary>
        public Shape Shape => m_Shape;
        
        private Bounds m_Bound;
        /// <summary>
        /// The collider's bounds, encompassing the whole shape
        /// </summary>
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

        /// <summary>
        /// Update the collider's shape, given the entitie's rotation and scale
        /// </summary>
        public bool UpdateShape(float transformRotation, Vector2 scale) {
            if (m_CachedRotation == (this.Rotation + transformRotation) % 360
                && m_CachedScale == scale
                && m_CachedBaseShape.Equals(this.BaseShape))
                return false;
            
            float targetRotation = (this.Rotation + transformRotation) % 360;

            m_Shape = new Shape(this.BaseShape, targetRotation, scale * this.Size);

            m_CachedRotation = targetRotation;
            m_CachedBaseShape = this.BaseShape;
            m_CachedScale = scale;

            m_Bound.Update(m_Shape);

            return true;
        }

        public override Component Clone() => new Collider(this);
    }
}
