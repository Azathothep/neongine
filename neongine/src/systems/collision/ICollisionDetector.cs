using System;

namespace neongine {
    public interface ICollisionDetector
    {
        public (GeometryType, GeometryType) Shapes { get; }

        public bool Collide(Point p1, Collider c1, Shape s1, Point p2, Collider c2, Shape s2);
        public bool Collide(Point p1, Collider c1, Shape s1, Point p2, Collider c2, Shape s2, out Collision collision);
    }
}
