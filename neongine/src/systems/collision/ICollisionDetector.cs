using System;
using Microsoft.Xna.Framework;

namespace neongine {
    public interface ICollisionDetector
    {
        public (GeometryType, GeometryType) Shapes { get; }

        public bool Collide(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2);
        public bool Collide(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2, out Collision collision);
    }
}
