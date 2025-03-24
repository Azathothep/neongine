using System;

namespace neongine {
    public interface ICollisionDetector
    {
        public (Shape.Type, Shape.Type) Shapes { get; }

        public bool Collide(Collidable collidable1, Collidable collidable2, out Collision collision);
    }
}
