using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    public class SATCollisionDetector : ICollisionDetector
    {
        private struct DetectedCollisions() {
            public (int, int)[] CollisionIndices;
            public Collision[] Collisions;
            public (int, int)[] TriggersIndices;
        }

        public CollisionDetectionData Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds)
        {
            (int, int)[] crossingBounds = BoundDetections(partition, ids, positions, bounds);

            DetectedCollisions detections = ShapeDetections(crossingBounds, positions, colliders, shapes);

            CollisionDetectionData datas = Convert(detections, ids);

            return datas;
        }

        private (int, int)[] BoundDetections(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Bounds[] bounds) {
            List<(int, int)> partitionIDs = new List<(int, int)>(partition.Count());

            foreach (var part in partition) {
                int i1 = Array.FindIndex(ids, id => id == part.Item1);
                int i2 = Array.FindIndex(ids, id => id == part.Item2);
                partitionIDs.Add((i1, i2));
            }
            
            List<(int, int)> crossingBounds = new();

            foreach ((int id1, int id2) in partitionIDs) {
                bool isCrossing = Bounds.Crossing(positions[id1], bounds[id1], positions[id2], bounds[id2]);

                if (isCrossing)
                    crossingBounds.Add((id1, id2));
            }

            return crossingBounds.ToArray();
        }

        private DetectedCollisions ShapeDetections((int, int)[] indices, Vector2[] positions, Collider[] colliders, Shape[] shapes) {
            List<(int, int)> collisionIndices = new();
            List<Collision> collisionList = new();
            List<(int, int)> triggerIndices = new();
            
            foreach ((int i1, int i2) in indices) {
                if (!colliders[i1].IsTrigger && !colliders[i2].IsTrigger) {
                    if (EvaluateCollision(positions[i1], colliders[i1], shapes[i1], positions[i2], colliders[i2], shapes[i2], out Collision collision)) {
                        collisionIndices.Add((i1, i2));
                        collisionList.Add(collision);
                    }
                } else if (EvaluateCollision(positions[i1], colliders[i1], shapes[i1], positions[i2], colliders[i2], shapes[i2])) {
                        triggerIndices.Add((i1, i2));
                }
            }

            DetectedCollisions data = new DetectedCollisions() {
                CollisionIndices = collisionIndices.ToArray(),
                Collisions = collisionList.ToArray(),
                TriggersIndices = triggerIndices.ToArray()
            };

            return data;
        }

        private bool EvaluateCollision(Vector2 p1, Collider c1, Shape s1, Vector2 p2, Collider c2, Shape s2) {
            if (!s1.IsPolygon && !s2.IsPolygon)
                return RadiusCollision.Collide(p1, s1.Radius, p2, s2.Radius);
            else if (s1.IsPolygon && s2.IsPolygon)
                return SeparatingAxisCollision.PolygonsCollide(p1, s1, p2, s2);
            else if (s1.IsPolygon && !s2.IsPolygon)
                return SeparatingAxisCollision.CircleCollide(p1, s1, p2, s2.Radius);
            else if (!s1.IsPolygon && s2.IsPolygon)
                return SeparatingAxisCollision.CircleCollide(p2, s2, p1, s1.Radius);

            Console.WriteLine($"Cannot evaluate collisions between {c1} and {c2}");

            return false;
        }

        private bool EvaluateCollision(Vector2 p1, Collider c1, Shape s1, Vector2 p2, Collider c2, Shape s2, out Collision collision) {
            if (!s1.IsPolygon && !s2.IsPolygon)
                return RadiusCollision.Collide(p1, s1.Radius, p2, s2.Radius, out collision);
            else if (s1.IsPolygon && s2.IsPolygon)
                return SeparatingAxisCollision.PolygonsCollide(p1, s1, p2, s2, out collision);
            else if (s1.IsPolygon && !s2.IsPolygon)
                return SeparatingAxisCollision.CircleCollide(p1, s1, p2, s2.Radius, out collision);
            else if (!s1.IsPolygon && s2.IsPolygon)
                return SeparatingAxisCollision.CircleCollide(p2, s2, p1, s1.Radius, out collision);

            Console.WriteLine($"Cannot evaluate collisions between {c1} and {c2}");

            collision = null;
            return false;
        }

        private CollisionDetectionData Convert(DetectedCollisions shapeDatas, EntityID[] ids) {
            CollisionData[] collisionData = new CollisionData[shapeDatas.CollisionIndices.Count()];

            for (int i = 0; i < shapeDatas.CollisionIndices.Count(); i++) {
                collisionData[i] = new CollisionData() {
                    Entities = (ids[shapeDatas.CollisionIndices[i].Item1], ids[shapeDatas.CollisionIndices[i].Item2]),
                    Collision = shapeDatas.Collisions[i]
                };
            }

            (EntityID, EntityID)[] triggers = new (EntityID, EntityID)[shapeDatas.TriggersIndices.Count()];

            for (int i = 0; i < shapeDatas.TriggersIndices.Length; i++)
                triggers[i] = (ids[shapeDatas.TriggersIndices[i].Item1], ids[shapeDatas.TriggersIndices[i].Item2]);

            CollisionDetectionData datas = new CollisionDetectionData() {
                Collisions = collisionData,
                Triggers = triggers
            };

            return datas;
        }
    }
}
