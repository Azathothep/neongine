using Microsoft.Xna.Framework;
using neon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace neongine
{
    public struct CollisionData {
        public (EntityID, EntityID) Entities;
        public Collision Collision;
    }

    public class CollisionSystem : IGameUpdateSystem
    {
        public bool ActiveInPlayMode => true;

        private static CollisionSystem instance;

#region definitions
        private struct QueryResultSOA {
            public int Length;
            public EntityID[] IDs;
            public Vector2[] Positions;
            public float[] Rotations;
            public Vector2[] Scales;
            public Transform[] Transforms;
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
                Transforms = new Transform[count];
                Velocities = new Velocity[count];
                Colliders = new Collider[count];
                Shapes = new Shape[count];
                Bounds = new Bounds[count];
                IsStatic = new bool[count];
            }
        }

        private struct FrameDataStorage {
            public Dictionary<(EntityID, EntityID), Collision> CollisionPairs = new();
            public Dictionary<EntityID, List<(EntityID, Collision)>> EntityToCollisions = new();
            public HashSet<(EntityID, EntityID)> TriggerPairs = new();
            public Dictionary<EntityID, List<EntityID>> EntityToTriggers = new();

            public FrameDataStorage() { }
        }
#endregion

        private Query<Transform, Collider, Velocity, IsStatic> m_Query = new(
            [
                new QueryFilter<Velocity>(FilterTerm.MightHave),
                new QueryFilter<IsStatic>(FilterTerm.MightHave)
            ]);

        private ISpacePartitioner m_SpacePartitioner;
        private ICollisionDetector m_CollisionDetector;
        private ICollisionResolver m_CollisionResolver;
        private FrameDataStorage m_Storage = new();

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
            IEnumerable<(EntityID, Transform, Collider, Velocity, IsStatic)> queryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Safe);

            QueryResultSOA query = Convert(queryResult, (float)timeSpan.TotalSeconds);

            for (int i = 0; i < query.Length; i++) {
                query.Colliders[i].UpdateShape(query.Rotations[i], query.Scales[i]);
            }

            IEnumerable<(EntityID, EntityID)> partition = m_SpacePartitioner.Partition(query.IDs, query.Positions, query.Bounds);

            Separate(partition, query.IDs, query.Colliders, out (EntityID, EntityID)[] collisionPartition, out (EntityID, EntityID)[] triggerPartition);

            m_CollisionDetector.Detect(collisionPartition, query.IDs, query.Positions, query.Colliders, query.Shapes, query.Bounds, out CollisionData[] collisionData);

            m_CollisionResolver.Resolve(collisionData, query.IDs, query.Transforms, query.Positions, query.Velocities, query.IsStatic, (float)timeSpan.TotalSeconds);

            (EntityID, EntityID)[] triggerData = m_CollisionDetector.Detect(triggerPartition, query.IDs, query.Positions, query.Colliders, query.Shapes, query.Bounds);

            FrameDataStorage currentStorage = Convert(collisionData, triggerData);

            TriggerEvents(currentStorage);

            m_Storage = currentStorage;
        }

        private QueryResultSOA Convert(IEnumerable<(EntityID, Transform, Collider, Velocity, IsStatic)> queryResult, float deltaTime) {
            int count = queryResult.Count();

            QueryResultSOA query = new QueryResultSOA(count);

            int index = 0;
            foreach ((EntityID id, Transform t, Collider c, Velocity v, IsStatic isStatic) in queryResult) {
                query.IDs[index] = id;
                query.Positions[index] = v == null ? t.WorldPosition2D : t.WorldPosition2D + v.Value * deltaTime;
                query.Rotations[index] = t.WorldRotation;
                query.Scales[index] = t.WorldScale;
                query.Transforms[index] = t;
                query.Velocities[index] = v;
                query.Colliders[index] = c;
                query.Shapes[index] = c.Shape;
                query.Bounds[index] = c.Bound;
                query.IsStatic[index] = isStatic != null;

                index++;
            }

            return query;
        }

        private void Separate(IEnumerable<(EntityID, EntityID)> partition, EntityID[] ids, Collider[] colliders, out (EntityID, EntityID)[] collisionPartition, out (EntityID, EntityID)[] triggerPartition)
        {
            List<(EntityID, EntityID)> collisionPartitionList = new();
            List<(EntityID, EntityID)> triggerPartitionList = new();

            foreach (var part in partition)
            {
                int i1 = Array.FindIndex(ids, id => id == part.Item1);
                int i2 = Array.FindIndex(ids, id => id == part.Item2);
                
                if (colliders[i1].IsTrigger || colliders[i2].IsTrigger)
                    triggerPartitionList.Add((part.Item1, part.Item2));
                else
                    collisionPartitionList.Add((part.Item1, part.Item2));
            }

            collisionPartition = collisionPartitionList.ToArray();
            triggerPartition = triggerPartitionList.ToArray();
        }

        private FrameDataStorage Convert(CollisionData[] collisionDatas, (EntityID, EntityID)[] triggerDatas) {
            FrameDataStorage storage = new FrameDataStorage();

            for (int i = 0; i < collisionDatas.Length; i++) {
                EntityID entity1 = collisionDatas[i].Entities.Item1;
                EntityID entity2 = collisionDatas[i].Entities.Item2;
                Collision collision = collisionDatas[i].Collision;

                storage.CollisionPairs.Add((entity1, entity2), collision);
    
                if (!storage.EntityToCollisions.TryGetValue(entity1, out List<(EntityID, Collision)> value1)) {
                    value1 = new List<(EntityID, Collision)>();
                    storage.EntityToCollisions.Add(entity1, value1);
                }

                if (!storage.EntityToCollisions.TryGetValue(entity2, out List<(EntityID, Collision)> value2)) {
                    value2 = new List<(EntityID, Collision)>();
                    storage.EntityToCollisions.Add(entity2, value2);
                }

                value1.Add((entity2, collision));
                value2.Add((entity1, collision));
            }

            for (int i = 0; i < triggerDatas.Length; i++) {
                (EntityID entity1, EntityID entity2) = triggerDatas[i];

                storage.TriggerPairs.Add(triggerDatas[i]);

                if (!storage.EntityToTriggers.TryGetValue(entity1, out List<EntityID> value1)) {
                    value1 = new List<EntityID>();
                    storage.EntityToTriggers.Add(entity1, value1);
                }

                if (!storage.EntityToTriggers.TryGetValue(entity2, out List<EntityID> value2)) {
                    value2 = new List<EntityID>();
                    storage.EntityToTriggers.Add(entity2, value2);
                }

                value1.Add(entity2);
                value2.Add(entity1);
            }

            return storage;
        }

        public static bool IsColliding(EntityID id)
        {
            return instance.m_Storage.EntityToCollisions.ContainsKey(id) || instance.m_Storage.EntityToTriggers.ContainsKey(id);
        }

        public static List<(EntityID, Collision)> GetCollisions(EntityID id)
        {
            if (instance.m_Storage.EntityToCollisions.TryGetValue(id, out var value))
                return value;
            return null;
        }

        public static List<EntityID> GetTriggers(EntityID id)
        {
            if (instance.m_Storage.EntityToTriggers.TryGetValue(id, out var value))
                return value;
            return null;
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
    }
}
