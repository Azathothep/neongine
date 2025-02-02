using neon;
using System;
using System.Collections;
using System.Collections.Generic;

namespace neongine
{
    [DoNotSerialize]
    public class CollisionSystem : IUpdateSystem, IDrawSystem
    {
        private Query<Point, Collider> m_Query = new();

        private ISpacePartitioner m_SpacePartitioner;

        private ICollisionDetector m_CollisionDetector;

        private ICollisionResolver m_CollisionResolver;

        private IEnumerable<(EntityID, Point, Collider)> m_QueryResult;

        private Dictionary<EntityID, Action<Collision>> m_OnCollisionEnter = new();
        private Dictionary<EntityID, Action<Collision>> m_OnCollisionExit = new();
        private Dictionary<EntityID, Action> m_OnTriggerEnter = new();
        private Dictionary<EntityID, Action> m_OnTriggerExit = new();

        public CollisionSystem(ISpacePartitioner spacePartitioner, ICollisionDetector collisionDetector, ICollisionResolver collisionResolver)
        {
            m_SpacePartitioner = spacePartitioner;
            m_CollisionDetector = collisionDetector;
            m_CollisionResolver = collisionResolver;
        }

        public void Update(TimeSpan timeSpan)
        {
            m_QueryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);
        }

        public void Draw()
        {

        }
    }
}
