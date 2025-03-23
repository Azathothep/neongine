using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Collider : IComponent
    {
        public EntityID EntityID { get; set; }

        [Serialize]
        private Shape m_Shape;
        public Shape Shape => m_Shape;

        [Serialize]
        private float m_Size = 1;
        public float Size => m_Size;

        public float Width => m_Shape.Width * m_Size;
        public float Height => m_Shape.Height * m_Size;

        [Serialize]
        public bool IsTrigger = false;

        public Collider() {
            m_Shape = new Shape(Shape.Type.Circle, 1);
            m_Size = 1;
            IsTrigger = false;
        }

        public Collider(Shape shape, int size = 1, bool isTrigger = false)
        {
            m_Shape = shape;
            m_Size = size;
            IsTrigger = isTrigger;
        }

        public Collider(Collider other)
        {
            m_Shape = other.m_Shape;
            m_Size = other.m_Size;
            IsTrigger = other.IsTrigger;
        }

        public IComponent Clone() => new Collider(this);
    }
}
