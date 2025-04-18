﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace neongine
{
    public class BaseCollisionProcessor : ICollisionProcessor
    {
        private Dictionary<(GeometryType, GeometryType), ICollisionDetector> m_CollisionDetectors = new();

        public BaseCollisionProcessor(ICollisionDetector[] detectors) {
            foreach (var detector in detectors)
                m_CollisionDetectors.Add(detector.Shapes, detector);
        }

        public void GetCollisions(IEnumerable<(int, int)> partition, Vector3[] positions, Collider[] colliders, ColliderShape[] shapes, ColliderBounds[] bounds, out ((int, int)[], Collision[]) collisions, out (int, int)[] triggers)
        {
            (int, int)[] crossingBounds = AABBCollisions(partition, positions, bounds);

            SATCollisions(crossingBounds, positions, colliders, shapes, out collisions, out triggers);
        }

        private (int, int)[] AABBCollisions(IEnumerable<(int, int)> partition, Vector3[] positions, ColliderBounds[] bounds) {
            List<(int, int)> crossingBounds = new();

            foreach ((int id1, int id2) in partition) {
                bool isCrossing = Bounds.Crossing(positions[id1], bounds[id1].Bounds, positions[id2], bounds[id2].Bounds);

                if (isCrossing)
                    crossingBounds.Add((id1, id2));
            }

            return crossingBounds.ToArray();
        }

        private void SATCollisions((int, int)[] indices, Vector3[] positions, Collider[] colliders, ColliderShape[] shapes, out ((int, int)[], Collision[]) collisions, out (int, int)[] triggers) {
            List<(int, int)> collisionIndices = new();
            List<Collision> collisionList = new();
            List<(int, int)> triggerIndices = new();
            
            foreach ((int i1, int i2) in indices) {
                if (!colliders[i1].IsTrigger && !colliders[i2].IsTrigger
                    && SATCollision(positions[i1], colliders[i1], shapes[i1].Shape, positions[i2], colliders[i2], shapes[i2].Shape, out Collision collision)) {
                    
                    collisionIndices.Add((i1, i2));
                    collisionList.Add(collision);
                }
                else if (SATCollision(positions[i1], colliders[i1], shapes[i1].Shape, positions[i2], colliders[i2], shapes[i2].Shape))
                {
                    triggerIndices.Add((i1, i2));
                }
            }

            collisions = (collisionIndices.ToArray(), collisionList.ToArray());
            triggers = triggerIndices.ToArray();
        }

        private bool SATCollision(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2) {
            ICollisionDetector detector;
            
            if (m_CollisionDetectors.TryGetValue((c1.Geometry.Type, c2.Geometry.Type), out detector))
                return detector.Collide(p1, c1, s1, p2, c2, s2);
            else if ((c1.Geometry.Type != c2.Geometry.Type)
                    && m_CollisionDetectors.TryGetValue((c2.Geometry.Type, c1.Geometry.Type), out detector))
                return detector.Collide(p2, c2, s2, p1, c1, s1);
            
            Console.WriteLine($"No collision detector for {c1.Geometry.Type} crossing {c2.Geometry.Type}");

            return false;
        }

        private bool SATCollision(Vector3 p1, Collider c1, Shape s1, Vector3 p2, Collider c2, Shape s2, out Collision collision) {
            ICollisionDetector detector;
            
            if (m_CollisionDetectors.TryGetValue((c1.Geometry.Type, c2.Geometry.Type), out detector))
                return detector.Collide(p1, c1, s1, p2, c2, s1, out collision);
            else if ((c1.Geometry.Type != c2.Geometry.Type)
                    && m_CollisionDetectors.TryGetValue((c2.Geometry.Type, c1.Geometry.Type), out detector))
                return detector.Collide(p2, c2, s2, p1, c1, s1, out collision);
            
            Debug.WriteLine($"No collision detector for {c1.Geometry.Type} crossing {c2.Geometry.Type}");

            collision = null;
            return false;
        }
    }
}
