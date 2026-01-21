using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    /// <summary>
    /// Implements ICollisionDetector to detect collisions using the Separating Axis Algorithm
    /// </summary>
    public class SATCollisionDetector : ICollisionDetector
    {
        /// <summary>
        /// Temporary datas to store informations about detected collisions
        /// </summary>
        private struct DetectedCollisions() {
            /// <summary>
            /// The indices of the Entities in the provided ids array
            /// </summary>
            public (int, int)[] CollisionIndices;

            /// <summary>
            /// The collision datas related to each <c>CollisionIndice</c>
            /// </summary>
            public Collision[] Collisions;
        }

        /// <summary>
        /// Detect collisions and fill a <c>CollisionData</c> array with the validated collisions.
        /// The first argument gives all the pairs we need to check in detecting collisions. This was filled by a <c>IPartitionSystem</c> previously called by the <c>CollisionSystem</c>.
        /// The <c>CollisionSystem</c> use SOA (Structure of Arrays) approach in storing datas.
        /// The array required as argument respect this structure. Thus, you can view the array indices as IDs : all the elements located at the same index in any array are related to the same entity.
        /// </summary>
        public void Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds, out CollisionData[] collisionData)
        {
            (int, int)[] crossingBounds = BoundDetections(partition, ids, positions, bounds);

            ShapeDetections(crossingBounds, positions, colliders, shapes, out DetectedCollisions detections);

            collisionData = Convert(detections, ids);
        }

        /// <summary>
        /// Detect collisions and fill array of tuples with the validated collisions.
        /// The first argument gives all the pairs we need to check in detecting collisions. This was filled by a <c>IPartitionSystem</c> previously called by the <c>CollisionSystem</c>.
        /// The <c>CollisionSystem</c> use SOA (Structure of Arrays) approach in storing datas.
        /// The array required as argument respect this structure. Thus, you can view the array indices as IDs : all the elements located at the same index in any array are related to the same entity.
        /// </summary>
        public (EntityID, EntityID)[] Detect(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Vector2[] positions, Collider[] colliders, Shape[] shapes, Bounds[] bounds)
        {
            (int, int)[] crossingBounds = BoundDetections(partition, ids, positions, bounds);

            ShapeDetections(crossingBounds, positions, colliders, shapes, out (int, int)[] detection);

            (EntityID, EntityID)[] crossingEntities = Convert(detection, ids);
        
            return crossingEntities;
        }

        /// <summary>
        /// Returns an array of tuples with every entity pairs that have overlapping bounds. This is a first filter before proper shape overlap detection.
        /// </summary>
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

        /// <summary>
        /// Fill a <c>DetectedCollisions</c> structure with the detected shapes overlap using SAT algorithm.
        /// </summary>
        private void ShapeDetections((int, int)[] indices, Vector2[] positions, Collider[] colliders, Shape[] shapes, out DetectedCollisions detectedCollisions) {
            List<(int, int)> collisionIndices = new();
            List<Collision> collisionList = new();
            
            foreach ((int i1, int i2) in indices) {
                if (EvaluateCollision(positions[i1], colliders[i1], shapes[i1], positions[i2], colliders[i2], shapes[i2], out Collision collision)) {
                    collisionIndices.Add((i1, i2));
                    collisionList.Add(collision);
                }
            }

            detectedCollisions = new DetectedCollisions() {
                CollisionIndices = collisionIndices.ToArray(),
                Collisions = collisionList.ToArray()
            };
        }

        /// <summary>
        /// Fill an array of tuples with the detected shapes overlap using SAT algorithm.
        /// </summary>
        private void ShapeDetections((int, int)[] indices, Vector2[] positions, Collider[] colliders, Shape[] shapes, out (int, int)[] detectedTriggers)
        {
            List<(int, int)> detectedTriggersList = new();
            
            foreach ((int i1, int i2) in indices) {
                if (EvaluateCollision(positions[i1], colliders[i1], shapes[i1], positions[i2], colliders[i2], shapes[i2])) {
                        detectedTriggersList.Add((i1, i2));
                }
            }

            detectedTriggers = detectedTriggersList.ToArray();
        }

        /// <summary>
        /// Evaluate collision between two colliders depending on their shape type
        /// </summary>
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

        /// <summary>
        /// Evaluate collision between two colliders depending on their shape type and fill a <c>Collision</c> object if applicable,
        /// </summary>
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

        /// <summary>
        /// Convert a <c>DetectedCollisions</c> structure into a <c>CollisionData</c> array
        /// </summary>
        private CollisionData[] Convert(DetectedCollisions shapeDatas, EntityID[] ids) {
            CollisionData[] collisionData = new CollisionData[shapeDatas.CollisionIndices.Count()];

            for (int i = 0; i < shapeDatas.CollisionIndices.Count(); i++) {
                collisionData[i] = new CollisionData() {
                    Entities = (ids[shapeDatas.CollisionIndices[i].Item1], ids[shapeDatas.CollisionIndices[i].Item2]),
                    Collision = shapeDatas.Collisions[i]
                };
            }

            return collisionData;
        }

        /// <summary>
        /// Convert a <c>DetectedCollisions</c> structure into an array of EntityID tuples
        /// </summary>
        private (EntityID, EntityID)[] Convert((int, int)[] crossingDatas, EntityID[] ids)
        {
            (EntityID, EntityID)[] triggers = new (EntityID, EntityID)[crossingDatas.Length];

            for (int i = 0; i < crossingDatas.Length; i++)
                triggers[i] = (ids[crossingDatas[i].Item1], ids[crossingDatas[i].Item2]);

            return triggers;
        }
    }
}
