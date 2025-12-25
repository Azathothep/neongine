#define DRAW_COLLISIONS

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using neon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace neongine
{
    public struct CollisionDetectionData {
        public CollisionData[] Collisions;
        public (EntityID, EntityID)[] Triggers;
    }

    public struct CollisionData {
        public (EntityID, EntityID) Entities;
        public Collision Collision;
    }

    public class CollisionSystem : IUpdateSystem, IDrawSystem
    {
        private static CollisionSystem instance;

#region definitions
        private struct QueryResultSOA {
            public int Length;
            public EntityID[] IDs;
            public Vector2[] Positions;
            public float[] Rotations;
            public Vector2[] Scales;
            public Point[] Points;
            public Velocity[] Velocities;
            public Collider[] Colliders;
            public Shape[] Shapes;
            public Bounds[] Bounds;
            public bool[] IsStatic;

            public QueryResultSOA(int count) {
                Length = count;
                IDs = new EntityID[count];
                Positions = new Vector2[count];
                Rotations = new float[count];
                Scales = new Vector2[count];
                Points = new Point[count];
                Velocities = new Velocity[count];
                Colliders = new Collider[count];
                Shapes = new Shape[count];
                Bounds = new Bounds[count];
                IsStatic = new bool[count];
            }
        }

        private struct FrameDataStorage {
            public HashSet<EntityID> IsColliding = new();
            public Dictionary<(EntityID, EntityID), Collision> CollisionPairs = new();
            public HashSet<(EntityID, EntityID)> TriggerPairs = new();

            public FrameDataStorage() { }
        }
#endregion

        private Query<Point, Collider, Velocity, IsStatic> m_Query = new(
            [
                new QueryFilter<Velocity>(FilterTerm.MightHave),
                new QueryFilter<IsStatic>(FilterTerm.MightHave)
            ]);

        private ISpacePartitioner m_SpacePartitioner;
        private ICollisionDetector m_CollisionDetector;
        private ICollisionResolver m_CollisionResolver;
        private FrameDataStorage m_Storage = new();
        private QueryResultSOA m_QueryResultArray = new();

#region events
        private Dictionary<EntityID, List<Action<EntityID, Collision>>> m_OnColliderEnter = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnColliderExit = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnTriggerEnter = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnTriggerExit = new();

        public static void OnColliderEnter(EntityID id, Action<EntityID, Collision> action) => SubscribeEvent(instance.m_OnColliderEnter, id, action);
        public static void OnColliderExit(EntityID id, Action<EntityID> action) => SubscribeEvent(instance.m_OnColliderExit, id, action);
        public static void OnTriggerEnter(EntityID id, Action<EntityID> action) => SubscribeEvent(instance.m_OnTriggerEnter, id, action);
        public static void OnTriggerExit(EntityID id, Action<EntityID> action) => SubscribeEvent(instance.m_OnTriggerExit, id, action);
        
        private static void SubscribeEvent<T>(Dictionary<EntityID, List<T>> dictionary, EntityID id, T action) {
            if (!dictionary.TryGetValue(id, out List<T> actions)) {
                actions = new List<T>();
                dictionary.Add(id, actions);
            }

            actions.Add(action);
        }
#endregion

        public CollisionSystem(ISpacePartitioner spacePartitioner, ICollisionDetector collisionDetector, ICollisionResolver collisionResolver)
        {
            instance = this;

            m_SpacePartitioner = spacePartitioner;
            m_CollisionDetector = collisionDetector;
            m_CollisionResolver = collisionResolver;
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, Point, Collider, Velocity, IsStatic)> queryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Safe);

            QueryResultSOA query = Convert(queryResult);

            for (int i = 0; i < query.Length; i++) {
                query.Colliders[i].UpdateShape(query.Rotations[i], query.Scales[i]);
            }

            IEnumerable<(EntityID, EntityID)> partition = m_SpacePartitioner.Partition(query.IDs, query.Positions, query.Bounds);

            CollisionDetectionData detectionData = m_CollisionDetector.Detect(partition, query.IDs, query.Positions, query.Colliders, query.Shapes, query.Bounds);

            m_CollisionResolver.Resolve(detectionData.Collisions, query.IDs, query.Velocities, query.IsStatic);

            FrameDataStorage currentStorage = Convert(detectionData);

            TriggerEvents(currentStorage);

            m_Storage = currentStorage;

            m_QueryResultArray = query;
        }

        private QueryResultSOA Convert(IEnumerable<(EntityID, Point, Collider, Velocity, IsStatic)> queryResult) {
            int count = queryResult.Count();

            QueryResultSOA query = new QueryResultSOA(count);

            int index = 0;
            foreach ((EntityID id, Point p, Collider c, Velocity v, IsStatic isStatic) in queryResult) {
                query.IDs[index] = id;
                query.Positions[index] = v == null ? p.WorldPosition2D : p.WorldPosition2D + v.Value;
                query.Rotations[index] = p.WorldRotation;
                query.Scales[index] = p.WorldScale;
                query.Points[index] = p;
                query.Velocities[index] = v;
                query.Colliders[index] = c;
                query.Shapes[index] = c.Shape;
                query.Bounds[index] = c.Bound;
                query.IsStatic[index] = isStatic != null;

                index++;
            }

            return query;
        }

        private FrameDataStorage Convert(CollisionDetectionData detectionDatas) {
            FrameDataStorage storage = new FrameDataStorage();

            for (int i = 0; i < detectionDatas.Collisions.Length; i++) {
                storage.CollisionPairs.Add(detectionDatas.Collisions[i].Entities, detectionDatas.Collisions[i].Collision);
                storage.IsColliding.Add(detectionDatas.Collisions[i].Entities.Item1);
                storage.IsColliding.Add(detectionDatas.Collisions[i].Entities.Item2);
            }

            for (int i = 0; i < detectionDatas.Triggers.Length; i++) {
                storage.TriggerPairs.Add(detectionDatas.Triggers[i]);
                storage.IsColliding.Add(detectionDatas.Triggers[i].Item1);
                storage.IsColliding.Add(detectionDatas.Triggers[i].Item2);
            }

            return storage;
        }

        private void TriggerEvents(FrameDataStorage currentStorage) {            
            Collision collision;

            // COLLIDER ENTER
            foreach (var currentCollision in currentStorage.CollisionPairs) {
                (EntityID id1, EntityID id2) = currentCollision.Key;

                if (!m_Storage.CollisionPairs.TryGetValue((id1, id2), out collision) && !m_Storage.CollisionPairs.TryGetValue((id2, id1), out collision))
                    SendEvents(id1, id2, collision, m_OnColliderEnter);
            }

            // TRIGGER ENTER
            foreach (var currentTrigger in currentStorage.TriggerPairs) {
                (EntityID id1, EntityID id2) = currentTrigger;

                if (!m_Storage.TriggerPairs.Contains((id1, id2)) && !m_Storage.TriggerPairs.Contains((id2, id1)))
                    SendEvents(id1, id2, m_OnTriggerEnter);
            }

            // COLLIDER EXIT
            foreach (var lastCollision in m_Storage.CollisionPairs) {
                (EntityID id1, EntityID id2) = lastCollision.Key;

                if (!currentStorage.CollisionPairs.ContainsKey((id1, id2)) && !currentStorage.CollisionPairs.ContainsKey((id2, id1)))
                    SendEvents(id1, id2, m_OnColliderExit);
            }

            // TRIGGER EXIT
            foreach (var lastTrigger in m_Storage.TriggerPairs) {
                (EntityID id1, EntityID id2) = lastTrigger; 

                if (!currentStorage.TriggerPairs.Contains((id1, id2)) && !currentStorage.TriggerPairs.Contains((id2, id1)))
                    SendEvents(id1, id2, m_OnTriggerExit);
            }
        }

        private void SendEvents(EntityID id1, EntityID id2, Dictionary<EntityID, List<Action<EntityID>>> actions) {
            if (actions.TryGetValue(id1, out var actions1))
                foreach (var action in actions1) action?.Invoke(id2);
                        
            if (actions.TryGetValue(id2, out var actions2))
                foreach (var action in actions2) action?.Invoke(id1);
        }

        private void SendEvents(EntityID id1, EntityID id2, Collision collision, Dictionary<EntityID, List<Action<EntityID, Collision>>> actions) {
            if (actions.TryGetValue(id1, out var actions1))
                foreach (var action in actions1) action?.Invoke(id2, collision);
                        
            if (actions.TryGetValue(id2, out var actions2))
                foreach (var action in actions2) action?.Invoke(id1, collision);
        }

        public void Draw()
        {
#if DRAW_COLLISIONS

            for (int i = 0; i < m_QueryResultArray.Length; i++) {
                (EntityID id, Vector2 pos, Point p, Collider c, Shape s, Bounds b) = (      m_QueryResultArray.IDs[i],
                                                                                            m_QueryResultArray.Positions[i],
                                                                                            m_QueryResultArray.Points[i],
                                                                                            m_QueryResultArray.Colliders[i],
                                                                                            m_QueryResultArray.Shapes[i],
                                                                                            m_QueryResultArray.Bounds[i]);

                Color color = m_Storage.IsColliding.Contains(id) ? Color.Red : Color.Yellow;

                if (c.BaseShape.IsPolygon)
                    RenderingSystem.DrawPolygon(p.WorldPosition2D, s.Vertices, color);
                else
                    RenderingSystem.DrawCircle(p.WorldPosition2D, s.Radius, color);

                // RenderingSystem.DrawBounds(p, b);
            }
#endif
        }
    }
}
