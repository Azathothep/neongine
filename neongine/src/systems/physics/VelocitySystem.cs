using Microsoft.Xna.Framework;
using neongine;
using System.Collections;
using System.Collections.Generic;
using neon;
using System;
using System.Diagnostics;

namespace neongine
{
    [Serialize, Order(OrderType.After, typeof(CollisionSystem))]
    public class VelocitySystem : IGameUpdateSystem
    {
        private Query<Point, Velocity> m_Query;

        public VelocitySystem()
        {
            m_Query = new Query<Point, Velocity>();
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, Point, Velocity)> qResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            foreach (var r in qResult)
            {
                Point p = r.Item2;
                Velocity v = r.Item3;

                p.WorldPosition = p.WorldPosition + new Vector3(v.Value.X, v.Value.Y, 0) * (float)timeSpan.TotalSeconds;
            }
        }
    }
}
