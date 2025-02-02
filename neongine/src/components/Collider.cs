using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class Collider : IComponent
    {
        public EntityID EntityID { get; set; }

        [Serialize]
        private IShape m_Shape;
        public IShape Shape => m_Shape;

        [Serialize]
        public bool IsTrigger;

        public Collider(IShape shape, bool isTrigger = false)
        {
            m_Shape = shape;
            IsTrigger = isTrigger;
        }

        public Collider(Collider other)
        {
            m_Shape = other.m_Shape.Clone();
        }

        public IComponent Clone() => new Collider(this);
    }
}
