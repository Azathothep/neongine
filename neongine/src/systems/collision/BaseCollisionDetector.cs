using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using neon;

namespace neongine
{
    public class BaseCollisionProcessor : ICollisionProcessor
    {
        private Dictionary<(Shape.Type, Shape.Type), ICollisionDetector> m_CollisionDetectors = new();

        public BaseCollisionProcessor(ICollisionDetector[] detectors) {
            foreach (var detector in detectors)
                m_CollisionDetectors.Add(detector.Shapes, detector);
        }

        public IEnumerable<Collision> GetCollisions(Collidable[] collidables, Bound[] bounds)
        {
            (int, int)[] crossingBounds = AABBCollisions(bounds);

            return SATCollisions(collidables, crossingBounds);
        }

        private (int, int)[] AABBCollisions(Bound[] bounds) {
            List<(int, int)> crossingBounds = new();

            for (int i = 0; i + 1 < bounds.Length; i++) {
                for (int y = i + 1; y < bounds.Length; y++) {
                    bool isCrossing = Bound.IsCrossing(bounds[i], bounds[y]);
                    
                    Debug.WriteLine($"Bounds crossing : {isCrossing}");

                    if (isCrossing)
                        crossingBounds.Add((i, y));
                }
            }

            return crossingBounds.ToArray();
        }

        private List<Collision> SATCollisions(Collidable[] collidables, (int, int)[] indices) {
            List<Collision> collisions = new();
            
            foreach ((int i1, int i2) in indices) {

                if (SATCollision(collidables[i1], collidables[i2], out Collision collision)) {
                    collisions.Add(collision);
                }
            }
            
            return collisions;
        }

        private bool SATCollision(Collidable collidable1, Collidable collidable2, out Collision collision) {
            ICollisionDetector detector;
            
            if (m_CollisionDetectors.TryGetValue((collidable1.Collider.Shape.ShapeType, collidable2.Collider.Shape.ShapeType), out detector))
                return detector.Collide(collidable1, collidable2, out collision);
            else if ((collidable1.Collider.Shape.ShapeType != collidable2.Collider.Shape.ShapeType)
                        && m_CollisionDetectors.TryGetValue((collidable2.Collider.Shape.ShapeType, collidable1.Collider.Shape.ShapeType), out detector))
                return detector.Collide(collidable2, collidable1, out collision);

            
            Debug.WriteLine($"No collision detector for {collidable1.Collider.Shape.ShapeType} crossing {collidable2.Collider.Shape.ShapeType}");

            collision = null;
            return false;
        }
    }
}
