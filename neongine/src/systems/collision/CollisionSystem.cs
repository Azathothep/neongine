#define DRAW_COLLISIONS

using Microsoft.Xna.Framework.Graphics;
using neon;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Linq;

namespace neongine
{
    public struct Bound {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Bound(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public static Bound Get(Shape shape, Vector2 size, float rotation) {
            switch (shape.ShapeType) {
                case Shape.Type.Circle:
                    float radius = shape.Width * size.X;
                    return new Bound((int)-radius, (int)-radius, (int)radius * 2, (int)radius * 2);
                default:
                    Vector2[] points = Shape.RotatePoints(shape, size, rotation);
                    
                    float xMin = float.MaxValue;
                    float xMax = float.MinValue;
                    float yMin = float.MaxValue;
                    float yMax = float.MinValue;

                    foreach (var point in points) {
                        if (point.X < xMin)
                            xMin = point.X;
                        if (point.X > xMax)
                            xMax = point.X;
                        if (point.Y < yMin)
                            yMin = point.Y;
                        if (point.Y > yMax)
                            yMax = point.Y;                    
                    }

                    return new Bound(xMin, yMin, xMax - xMin, yMax - yMin);
            }
        }
    }

    [DoNotSerialize]
    public class CollisionSystem : IUpdateSystem, IDrawSystem
    {
        private static CollisionSystem m_Instance;

        private Query<Point, Collider> m_Query = new();

        private ISpacePartitioner m_SpacePartitioner;

        private ICollisionProcessor m_CollisionProcessor;

        private ICollisionResolver m_CollisionResolver;

        private SpriteBatch m_SpriteBatch;

        private (EntityID, Point, Collider)[] m_QueryResultArray;

        private Bound[] m_LastBounds = new Bound[0];

        private HashSet<EntityID> m_IsColliding = new();
        private Dictionary<(EntityID, EntityID), Collision> m_LastCollisions = new();

        private Dictionary<EntityID, List<Action<Collision>>> m_OnColliderEnter = new();
        private Dictionary<EntityID, List<Action<Collision>>> m_OnColliderExit = new();
        private Dictionary<EntityID, List<Action<Collision>>> m_OnTriggerEnter = new();
        private Dictionary<EntityID, List<Action<Collision>>> m_OnTriggerExit = new();

        public static void OnColliderEnter(EntityID id, Action<Collision> action) => SubscribeEvent(m_Instance.m_OnColliderEnter, id, action);
        public static void OnColliderExit(EntityID id, Action<Collision> action) => SubscribeEvent(m_Instance.m_OnColliderExit, id, action);
        public static void OnTriggerEnter(EntityID id, Action<Collision> action) => SubscribeEvent(m_Instance.m_OnTriggerEnter, id, action);
        public static void OnTriggerExit(EntityID id, Action<Collision> action) => SubscribeEvent(m_Instance.m_OnTriggerExit, id, action);

        private static void SubscribeEvent(Dictionary<EntityID, List<Action<Collision>>> dictionary, EntityID id, Action<Collision> action) {
            if (!dictionary.TryGetValue(id, out List<Action<Collision>> actions)) {
                actions = new List<Action<Collision>>();
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
            HashSet<EntityID> isColliding = new();
            Dictionary<(EntityID, EntityID), Collision> collisions = new();

            m_QueryResultArray = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe).ToArray();

            m_LastBounds = new Bound[m_QueryResultArray.Length];

            for (int i = 0; i < m_QueryResultArray.Length; i++) {
                (EntityID _, Point p, Collider c) = m_QueryResultArray[i];
                m_LastBounds[i] = Bound.Get(c.Shape, c.Size * p.WorldScale, p.WorldRotation);
            }

            (EntityID, Point, Collider, Bound)[][] partitions = m_SpacePartitioner.Partition(m_QueryResultArray, m_LastBounds);

            foreach (var partition in partitions) {
                for (int i = 0; i + 1 < partition.Length; i++) {
                    (EntityID id1, Point p1, Collider c1, Bound b1) = partition[i];
                    (EntityID id2, Point p2, Collider c2, Bound b2) = partition[i + 1];

                    bool crossingBounds = CrossBounds(p1, b1, p2, b2);
                    Debug.WriteLine($"Bounds crossing : {crossingBounds}");
                    if (crossingBounds == false)
                        continue;
                        
                    bool collide = m_CollisionProcessor.Collide(p1, c1, p2, c2, out Collision collision);

                    if (collide) {
                        Debug.WriteLine($"Collision detected for {c1.Shape.ShapeType} crossing {c2.Shape.ShapeType}");

                        isColliding.Add(id1);
                        isColliding.Add(id2);

                        collisions.Add((id1, id2), collision);
                    }
                }
            }

            foreach (var collision in collisions) {
                (EntityID id1, EntityID id2) = collision.Key;
                if (!m_LastCollisions.TryGetValue((id1, id2), out Collision _)
                    && !m_LastCollisions.TryGetValue((id2, id1), out Collision _)) {

                    if (collision.Value.Datas.Item1.collider.IsTrigger || collision.Value.Datas.Item2.collider.IsTrigger) {
                        if (m_OnTriggerEnter.TryGetValue(id1, out List<Action<Collision>> onTriggerEnterId1))
                            foreach (var action in onTriggerEnterId1) action?.Invoke(collision.Value);
                                
                        if (m_OnTriggerEnter.TryGetValue(id2, out List<Action<Collision>> onTriggerEnterId2))
                            foreach (var action in onTriggerEnterId2) action?.Invoke(collision.Value);
                    } else {
                        if (m_OnColliderEnter.TryGetValue(id1, out List<Action<Collision>> onCollisionEnterId1))
                            foreach (var action in onCollisionEnterId1) action?.Invoke(collision.Value);
                                
                        if (m_OnColliderEnter.TryGetValue(id2, out List<Action<Collision>> onCollisionEnterId2))
                            foreach (var action in onCollisionEnterId2) action?.Invoke(collision.Value);
                    }
                }
            }

            foreach (var lastCollision in m_LastCollisions) {
                (EntityID id1, EntityID id2) = lastCollision.Key;
                if (!collisions.TryGetValue((id1, id2), out Collision _)
                    && !collisions.TryGetValue((id2, id1), out Collision _)) {

                    if (lastCollision.Value.Datas.Item1.collider.IsTrigger || lastCollision.Value.Datas.Item2.collider.IsTrigger) {
                        if (m_OnTriggerExit.TryGetValue(id1, out List<Action<Collision>> onTriggerExitId1))
                            foreach (var action in onTriggerExitId1) action?.Invoke(lastCollision.Value);
                                
                        if (m_OnTriggerExit.TryGetValue(id2, out List<Action<Collision>> onTriggerExitId2))
                            foreach (var action in onTriggerExitId2) action?.Invoke(lastCollision.Value);
                    } else {
                        if (m_OnColliderExit.TryGetValue(id1, out List<Action<Collision>> onCollisionExitId1))
                            foreach (var action in onCollisionExitId1) action?.Invoke(lastCollision.Value);
                                
                        if (m_OnColliderExit.TryGetValue(id2, out List<Action<Collision>> onCollisionExitId2))
                            foreach (var action in onCollisionExitId2) action?.Invoke(lastCollision.Value);
                    }
                }
            }

            foreach (var collision in collisions) {
                if (!collision.Value.Datas.Item1.collider.IsTrigger && !collision.Value.Datas.Item2.collider.IsTrigger)
                m_CollisionResolver.Resolve(collision.Value);
            }

            m_IsColliding = isColliding;
            m_LastCollisions = collisions;
        }

        private bool CrossBounds(Point p1, Bound b1, Point p2, Bound b2) {
            (Point lP, Bound lB, Point rP, Bound rB) = p1.WorldPosition.X + b1.X < p2.WorldPosition.X + b2.X ? (p1, b1, p2, b2) : (p2, b2, p1, b1);
            (Point tP, Bound tB, Point bP, Bound bB) = p1.WorldPosition.Y + b1.Y < p2.WorldPosition.Y + b2.Y ? (p1, b1, p2, b2) : (p2, b2, p1, b1);

            return ((rP.WorldPosition.X + rB.X) <= (lP.WorldPosition.X + lB.Width / 2))
            && ((bP.WorldPosition.Y + bB.Y) <= (tP.WorldPosition.Y + tB.Height / 2));
        }

        public void Draw()
        {
#if DRAW_COLLISIONS

            m_SpriteBatch.Begin();

            for (int i = 0; i < m_QueryResultArray.Length; i++) {
                (EntityID id, Point p, Collider c) = m_QueryResultArray[i];
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

                Bound bound = m_LastBounds[i];
                MonoGame.Primitives2D.DrawRectangle(m_SpriteBatch,
                                                        new Rectangle((int)(p.WorldPosition.X + bound.X),
                                                                    (int)(p.WorldPosition.Y + bound.Y),
                                                                    (int)bound.Width,
                                                                    (int)bound.Height),
                                                        0.0f,
                                                        Color.Blue);
            }

            m_SpriteBatch.End();
#endif
        }
    }
}
