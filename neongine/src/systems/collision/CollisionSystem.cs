#define DRAW_COLLISIONS

using Microsoft.Xna.Framework.Graphics;
using neon;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace neongine
{
    [DoNotSerialize]
    public class CollisionSystem : IUpdateSystem, IDrawSystem
    {
        private static CollisionSystem m_Instance;

        private Query<Point, Collider> m_Query = new();

        private ISpacePartitioner m_SpacePartitioner;

        private ICollisionProcessor m_CollisionProcessor;

        private ICollisionResolver m_CollisionResolver;

        private SpriteBatch m_SpriteBatch;

        private IEnumerable<(EntityID, Point, Collider)> m_QueryResult;

        private Collidable[] m_LastCollidables = new Collidable[0];

        private HashSet<EntityID> m_IsColliding = new();
        private Dictionary<(EntityID, EntityID), Collision> m_LastCollisionPairs = new();
        private Dictionary<(EntityID, EntityID), Collision> m_LastTriggerPairs = new();

        private Dictionary<EntityID, List<Action<Collision>>> m_OnColliderEnter = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnColliderExit = new();
        private Dictionary<EntityID, List<Action<Collision>>> m_OnTriggerEnter = new();
        private Dictionary<EntityID, List<Action<EntityID>>> m_OnTriggerExit = new();

        public static void OnColliderEnter(EntityID id, Action<Collision> action) => SubscribeEvent(m_Instance.m_OnColliderEnter, id, action);
        public static void OnColliderExit(EntityID id, Action<EntityID> action) => SubscribeEvent(m_Instance.m_OnColliderExit, id, action);
        public static void OnTriggerEnter(EntityID id, Action<Collision> action) => SubscribeEvent(m_Instance.m_OnTriggerEnter, id, action);
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
            m_QueryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            Collidable[] collidables = new Collidable[m_QueryResult.Count()];

            int i = 0;
            foreach ((EntityID id, Point p, Collider c) in m_QueryResult) {
                Bound bound = Bound.Get(c.Shape, p.WorldPosition2D, c.Size * p.WorldScale, p.WorldRotation);
                collidables[i] = new Collidable(id, p, c, bound);
                i++;
            }

            Collidable[][] partitions = m_SpacePartitioner.Partition(collidables);

            List<Collision> collisions = new();

            for (int j = 0; j < partitions.Length; j++) {
                IEnumerable<Collision> partCollisions = m_CollisionProcessor.GetCollisions(partitions[j]);
                collisions.AddRange(partCollisions);
            }

            Dictionary<(EntityID, EntityID), Collision> currentCollisionsPairs;
            Dictionary<(EntityID, EntityID), Collision> currentTriggersPairs;
            HashSet<EntityID> isColliding;

            GetCollisionPairs(collisions, out currentCollisionsPairs, out currentTriggersPairs, out isColliding);

            foreach (var collision in collisions) {
                if (!collision.Collidable1.Collider.IsTrigger && !collision.Collidable2.Collider.IsTrigger)
                    m_CollisionResolver.Resolve(collision);
            }

            TriggerEvents(currentTriggersPairs, currentCollisionsPairs);

            m_LastCollidables = collidables;
            m_LastTriggerPairs = currentTriggersPairs;
            m_LastCollisionPairs = currentCollisionsPairs;
            m_IsColliding = isColliding;
        }

        private void GetCollisionPairs(List<Collision> collisions, out Dictionary<(EntityID, EntityID), Collision> currentCollisionsPairs, out Dictionary<(EntityID, EntityID), Collision> currentTriggersPairs, out HashSet<EntityID> isColliding) {
            currentTriggersPairs = new();
            currentCollisionsPairs = new();
            isColliding = new();
            
            foreach (var collision in collisions) {
                (EntityID id1, EntityID id2) = (collision.Collidable1.EntityID, collision.Collidable2.EntityID);

                if (collision.Collidable1.Collider.IsTrigger || collision.Collidable2.Collider.IsTrigger)
                    currentTriggersPairs.Add((id1, id2), collision);
                else
                    currentCollisionsPairs.Add((id1, id2), collision);

                isColliding.Add(id1);
                isColliding.Add(id2);
            }
        }

        private void TriggerEvents(Dictionary<(EntityID, EntityID), Collision> currentTriggersPairs, Dictionary<(EntityID, EntityID), Collision> currentCollisionsPairs) {            
            Collision collision;

            // COLLIDER ENTER
            foreach (var currentCollision in currentCollisionsPairs) {
                (EntityID id1, EntityID id2) = currentCollision.Key;

                if (!m_LastCollisionPairs.TryGetValue((id1, id2), out collision) && !m_LastCollisionPairs.TryGetValue((id2, id1), out collision))
                    TriggerEnterEvents(id1, id2, collision, m_OnColliderEnter);
            }

            // TRIGGER ENTER
            foreach (var currentTrigger in currentTriggersPairs) {
                (EntityID id1, EntityID id2) = currentTrigger.Key;

                if (!m_LastTriggerPairs.TryGetValue((id1, id2), out collision) && !m_LastTriggerPairs.TryGetValue((id2, id1), out collision))
                    TriggerEnterEvents(id1, id2, collision, m_OnTriggerEnter);
            }

            // COLLIDER EXIT
            foreach (var lastCollision in m_LastCollisionPairs) {
                (EntityID id1, EntityID id2) = lastCollision.Key;

                if (!currentCollisionsPairs.ContainsKey((id1, id2)) && !currentCollisionsPairs.ContainsKey((id2, id1)))
                    TriggerExitEvents(id1, id2, m_OnColliderExit);
            }

            // TRIGGER EXIT
            foreach (var lastTrigger in m_LastTriggerPairs) {
                (EntityID id1, EntityID id2) = lastTrigger.Key; 

                if (!currentTriggersPairs.ContainsKey((id1, id2)) && !currentTriggersPairs.ContainsKey((id2, id1)))
                    TriggerExitEvents(id1, id2, m_OnTriggerExit);
            }
        }

        private void TriggerEnterEvents(EntityID id1, EntityID id2, Collision collision, Dictionary<EntityID, List<Action<Collision>>> onTrigger) {
            if (onTrigger.TryGetValue(id1, out var actions1))
                foreach (var action in actions1) action?.Invoke(collision);
                        
            if (onTrigger.TryGetValue(id2, out var actions2))
                foreach (var action in actions2) action?.Invoke(collision);
        }

        private void TriggerExitEvents(EntityID id1, EntityID id2, Dictionary<EntityID, List<Action<EntityID>>> onTrigger) {
            if (onTrigger.TryGetValue(id1, out var actions1))
                foreach (var action in actions1) action?.Invoke(id2);
                        
            if (onTrigger.TryGetValue(id2, out var actions2))
                foreach (var action in actions2) action?.Invoke(id1);
        }

        public void Draw()
        {
#if DRAW_COLLISIONS

            m_SpriteBatch.Begin();

            foreach (Collidable collidable in m_LastCollidables) {
                (EntityID id, Point p, Collider c, Bound b) = (collidable.EntityID, collidable.Point, collidable.Collider, collidable.Bound);
                switch (c.Shape.ShapeType) {
                    case Shape.Type.Circle:
                        MonoGame.Primitives2D.DrawCircle(m_SpriteBatch,
                                                    new Vector2(p.WorldPosition.X, p.WorldPosition.Y),
                                                    c.Width * p.WorldScale.X,
                                                    8,
                                                    m_IsColliding.Contains(id) ? Color.Red : Color.Yellow);
                        break;

                    case Shape.Type.Rectangle:
                        float screenWidth = c.Width * p.WorldScale.X;
                        float screenHeight = c.Height * p.WorldScale.Y;
                        MonoGame.Primitives2D.DrawRectangle(m_SpriteBatch,
                                                        new Rectangle((int)(p.WorldPosition.X - screenWidth / 2),
                                                                    (int)(p.WorldPosition.Y - screenHeight / 2),
                                                                    (int)screenWidth,
                                                                    (int)screenHeight),
                                                        p.WorldRotation,
                                                        m_IsColliding.Contains(id) ? Color.Red : Color.Yellow);
                        break;

                    default:
                        break;
                }

                MonoGame.Primitives2D.DrawRectangle(m_SpriteBatch,
                                                        new Rectangle((int)(p.WorldPosition.X + b.X),
                                                                    (int)(p.WorldPosition.Y + b.Y),
                                                                    (int)b.Width,
                                                                    (int)b.Height),
                                                        0.0f,
                                                        Color.Blue);
            }

            m_SpriteBatch.End();
#endif
        }
    }
}
