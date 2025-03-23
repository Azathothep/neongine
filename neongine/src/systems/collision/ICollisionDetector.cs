using System;

namespace neongine {
    public interface ICollisionDetector
    {
        public (Shape.Type, Shape.Type) Shapes { get; }

        public bool Collide(Point p1, Collider c1, Point p2, Collider c2, out Collision collision);
    }
}
