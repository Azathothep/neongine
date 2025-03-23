using System.Collections.Generic;
using System.Diagnostics;

namespace neongine
{
    public class BaseCollisionProcessor : ICollisionProcessor
    {
        private Dictionary<(Shape.Type, Shape.Type), ICollisionDetector> m_CollisionDetectors = new();

        public BaseCollisionProcessor(ICollisionDetector[] detectors) {
            foreach (var detector in detectors)
                m_CollisionDetectors.Add(detector.Shapes, detector);
        }

        public bool Collide(Point p1, Collider c1, Point p2, Collider c2, out Collision collision)
        {
            if (m_CollisionDetectors.TryGetValue((c1.Shape.ShapeType, c2.Shape.ShapeType), out ICollisionDetector detector1))
                return detector1.Collide(p1, c1, p2, c2, out collision);
            else if (   (c1.Shape.ShapeType != c2.Shape.ShapeType)
                        && m_CollisionDetectors.TryGetValue((c2.Shape.ShapeType, c1.Shape.ShapeType), out ICollisionDetector detector2))
                return detector2.Collide(p2, c2, p1, c1, out collision);

            
            Debug.WriteLine($"No collision detector for {c1.Shape.ShapeType} crossing {c2.Shape.ShapeType}");
            collision = null;
            return false;
        }
    }
}
