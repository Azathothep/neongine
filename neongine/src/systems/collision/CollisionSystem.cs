﻿#define DRAW_COLLISIONS

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using neon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace neongine
{
    public class CollisionSystem : IUpdateSystem, IDrawSystem
    {
        private static CollisionSystem m_Instance;

        private Query<Point, Collider, ColliderShape, ColliderBounds, Velocity, IsStatic> m_Query = new(
            [
                new QueryFilter<ColliderShape>(FilterTerm.MightHave),
                new QueryFilter<ColliderBounds>(FilterTerm.MightHave),
                new QueryFilter<Velocity>(FilterTerm.MightHave),
                new QueryFilter<IsStatic>(FilterTerm.MightHave)
            ]);

        private ISpacePartitioner m_SpacePartitioner;
        private ICollisionProcessor m_CollisionProcessor;
        private ICollisionResolver m_CollisionResolver;

        private SpriteBatch m_SpriteBatch;

        private HashSet<EntityID> m_IsColliding = new();
        private Dictionary<(EntityID, EntityID), Collision> m_LastCollisionPairs = new();
        private HashSet<(EntityID, EntityID)> m_LastTriggerPairs = new();

        private struct QueryResultArray {
            public int Length;
            public EntityID[] IDs;
            public Vector3[] Positions;
            public float[] Rotations;
            public Vector2[] Scales;
            public Collider[] Colliders;
            public ColliderShape[] Shapes;
            public ColliderBounds[] Bounds;
            public bool[] IsStatic;
        }

        private QueryResultArray m_LastQueryResultArray = new();

        private Dictionary<EntityID, List<Action<EntityID, Collision>>> m_OnColliderEnter = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnColliderExit = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnTriggerEnter = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnTriggerExit = new();

        public static void OnColliderEnter(EntityID id, Action<EntityID, Collision> action) => SubscribeEvent(m_Instance.m_OnColliderEnter, id, action);
        public static void OnColliderExit(EntityID id, Action<EntityID> action) => SubscribeEvent(m_Instance.m_OnColliderExit, id, action);
        public static void OnTriggerEnter(EntityID id, Action<EntityID> action) => SubscribeEvent(m_Instance.m_OnTriggerEnter, id, action);
        public static void OnTriggerExit(EntityID id, Action<EntityID> action) => SubscribeEvent(m_Instance.m_OnTriggerExit, id, action);

        private static void SubscribeEvent<T>(Dictionary<EntityID, List<T>> dictionary, EntityID id, T action) {
            if (!dictionary.TryGetValue(id, out List<T> actions)) {
                actions = new List<T>();
                dictionary.Add(id, actions);
            }

            actions.Add(action);
        }

        public CollisionSystem(ISpacePartitioner spacePartitioner, ICollisionProcessor collisionProcessor, ICollisionResolver collisionResolver, SpriteBatch spriteBatch)
        {
            m_Instance = this;

            m_SpacePartitioner = spacePartitioner;
            m_CollisionProcessor = collisionProcessor;
            m_CollisionResolver = collisionResolver;
            m_SpriteBatch = spriteBatch;
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, Point, Collider, ColliderShape, ColliderBounds, Velocity, IsStatic)> queryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Safe);

            QueryResultArray query = MakeResultArray(queryResult);

            List<int> boundsToUpdate = new();

            for (int i = 0; i < query.Length; i++) {
                if (query.Shapes[i].Update(query.Colliders[i], query.Rotations[i], query.Scales[i]))
                    boundsToUpdate.Add(i);
            }

            foreach (var i in boundsToUpdate) {
                query.Bounds[i].Update(query.Shapes[i].Shape);
            }

            IEnumerable<(int, int)> partition = m_SpacePartitioner.Partition(query.Positions, query.Bounds);

            // Debug.WriteLine($"{partition.Count()} collisions to process for {query.Points.Length} entities");

            ((int, int)[], Collision[]) collisions;
            (int, int)[] triggers;

            m_CollisionProcessor.GetCollisions(partition, query.Positions, query.Colliders, query.Shapes, query.Bounds, out collisions, out triggers);

            m_CollisionResolver.Resolve(collisions.Item2);

            Dictionary<(EntityID, EntityID), Collision> collisionPairs;
            HashSet<(EntityID, EntityID)> triggerPairs;
            HashSet<EntityID> isColliding;

            GetCollisionPairs(query.IDs, collisions, triggers, out collisionPairs, out triggerPairs, out isColliding);

            TriggerEvents(triggerPairs, collisionPairs);

            m_LastTriggerPairs = triggerPairs;
            m_LastCollisionPairs = collisionPairs;
            m_IsColliding = isColliding;

            m_LastQueryResultArray = query;
        }

        private QueryResultArray MakeResultArray(IEnumerable<(EntityID, Point, Collider, ColliderShape, ColliderBounds, Velocity, IsStatic)> queryResult) {
            int count = queryResult.Count();

            QueryResultArray query = new QueryResultArray() {
                Length = count,
                IDs = new EntityID[count],
                Positions = new Vector3[count],
                Rotations = new float[count],
                Scales = new Vector2[count],
                Colliders = new Collider[count],
                Shapes = new ColliderShape[count],
                Bounds = new ColliderBounds[count],
                IsStatic = new bool[count]
            };

            int index = 0;
            foreach ((EntityID id, Point p, Collider c, ColliderShape s, ColliderBounds b, Velocity v, IsStatic isStatic) in queryResult) {
                query.IDs[index] = id;
                query.Positions[index] = v == null ? p.WorldPosition : p.WorldPosition + v.Value;
                query.Rotations[index] = p.WorldRotation;
                query.Scales[index] = p.WorldScale;
                query.Colliders[index] = c;
                query.Shapes[index] = s == null ? id.Add<ColliderShape>() : s;
                query.Bounds[index] = b == null ? id.Add<ColliderBounds>() : b;
                query.IsStatic[index] = isStatic != null;

                index++;
            }

            return query;
        }

        private void GetCollisionPairs(EntityID[] entityIDs, ((int, int)[], Collision[]) collisionDatas, (int, int)[] triggers, out Dictionary<(EntityID, EntityID), Collision> collisionPairs, out HashSet<(EntityID, EntityID)> triggerPairs, out HashSet<EntityID> isColliding) {
            triggerPairs = new();
            collisionPairs = new();
            isColliding = new();
            
            ((int, int)[] collisionIndices, Collision[] collisions) = collisionDatas;

            int i = 0;
            foreach ((int i1, int i2) in collisionIndices) {
                (EntityID id1, EntityID id2) = (entityIDs[i1], entityIDs[i2]);
                collisionPairs.Add((id1, id2), collisions[i]);
                isColliding.Add(id1);
                isColliding.Add(id2);
                i++;
            }

            foreach ((int i1, int i2) in triggers)
            {
                (EntityID id1, EntityID id2) = (entityIDs[i1], entityIDs[i2]);
                triggerPairs.Add((id1, id2));
                isColliding.Add(id1);
                isColliding.Add(id2);
            }
        }

        private void TriggerEvents(HashSet<(EntityID, EntityID)> currentTriggersPairs, Dictionary<(EntityID, EntityID), Collision> currentCollisionsPairs) {            
            Collision collision;

            // COLLIDER ENTER
            foreach (var currentCollision in currentCollisionsPairs) {
                (EntityID id1, EntityID id2) = currentCollision.Key;

                if (!m_LastCollisionPairs.TryGetValue((id1, id2), out collision) && !m_LastCollisionPairs.TryGetValue((id2, id1), out collision))
                    SendEvents(id1, id2, collision, m_OnColliderEnter);
            }

            // TRIGGER ENTER
            foreach (var currentTrigger in currentTriggersPairs) {
                (EntityID id1, EntityID id2) = currentTrigger;

                if (!m_LastTriggerPairs.Contains((id1, id2)) && !m_LastTriggerPairs.Contains((id2, id1)))
                    SendEvents(id1, id2, m_OnTriggerEnter);
            }

            // COLLIDER EXIT
            foreach (var lastCollision in m_LastCollisionPairs) {
                (EntityID id1, EntityID id2) = lastCollision.Key;

                if (!currentCollisionsPairs.ContainsKey((id1, id2)) && !currentCollisionsPairs.ContainsKey((id2, id1)))
                    SendEvents(id1, id2, m_OnColliderExit);
            }

            // TRIGGER EXIT
            foreach (var lastTrigger in m_LastTriggerPairs) {
                (EntityID id1, EntityID id2) = lastTrigger; 

                if (!currentTriggersPairs.Contains((id1, id2)) && !currentTriggersPairs.Contains((id2, id1)))
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

            m_SpriteBatch.Begin();

            for (int i = 0; i < m_LastQueryResultArray.Length; i++) {
                (EntityID id, Vector3 p, Collider c, ColliderShape s, ColliderBounds b) = (   m_LastQueryResultArray.IDs[i],
                                                                                            m_LastQueryResultArray.Positions[i],
                                                                                            m_LastQueryResultArray.Colliders[i],
                                                                                            m_LastQueryResultArray.Shapes[i],
                                                                                            m_LastQueryResultArray.Bounds[i]);

                Color color = m_IsColliding.Contains(id) ? Color.Red : Color.Yellow;

                switch (c.Geometry.Type) {
                    case GeometryType.Circle:
                        DrawCircle(p, s.Shape.Vertices[1].X, c, color);
                        break;
                    default:
                        DrawPolygon(p, s.Shape.Vertices, color);
                        break;
                }

                // DrawBounds(p, b.Bounds);
            }


            m_SpriteBatch.End();
#endif
        }

        private void DrawCircle(Vector3 p, float radius, Collider c, Color color) {
            MonoGame.Primitives2D.DrawCircle(m_SpriteBatch,
                            new Vector2(p.X, p.Y),
                            c.Size * radius,
                            8,
                            color);
        }

        private void DrawPolygon(Vector3 p, Vector2[] vertices, Color color) {
            Vector2 position2D = new Vector2(p.X, p.Y);
            for (int i = 0; i < vertices.Length - 1; i++) {
                MonoGame.Primitives2D.DrawLine(m_SpriteBatch, position2D + vertices[i], position2D + vertices[i + 1], color);
            }

            MonoGame.Primitives2D.DrawLine(m_SpriteBatch, position2D + vertices[vertices.Length - 1], position2D + vertices[0], color);
        }

        private void DrawBounds(Point p, Bounds bounds) {
            MonoGame.Primitives2D.DrawRectangle(m_SpriteBatch,
                new Rectangle((int)(p.WorldPosition.X + bounds.X),
                            (int)(p.WorldPosition.Y + bounds.Y),
                            (int)bounds.Width,
                            (int)bounds.Height),
                0.0f,
                Color.Blue);
        }
    }
}
