using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    /// <summary>
    /// Stores datas about the penetration axis and length of overlapping entities
    /// </summary>
    public struct Penetration {
        public Vector2 Axis;
        public float Length;
        public Penetration(Vector2 axis, float length) {
            Axis = axis;
            Length = length;
        }
    }

    /// <summary>
    /// Stores penetration datas relative to a collision situation
    /// </summary>
    public class Collision
    {
        public Penetration[] Penetration;

        public Collision() {
            
        }
    }

    /// <summary>
    /// Stores a <c>Collision</c> object along with the two entities involved
    /// </summary>
    public struct CollisionData {
        public (EntityID, EntityID) Entities;
        public Collision Collision;
    }
}
