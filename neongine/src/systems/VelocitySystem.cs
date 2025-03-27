using Microsoft.Xna.Framework;
using neongine;
using System.Collections;
using System.Collections.Generic;
using neon;
using System;
using System.Diagnostics;

namespace neongine
{
    public class VelocitySystem : IUpdateSystem
    {
        [Serialize]
        private float m_Speed;

        private Query<Point, Velocity> m_Query;

        public VelocitySystem(float speed)
        {
            m_Speed = speed;
            m_Query = new Query<Point, Velocity>();
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, Point, Velocity)> qResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            foreach (var r in qResult)
            {
                Point p = r.Item2;
                Velocity v = r.Item3;

                p.WorldPosition = p.WorldPosition + v.Direction * m_Speed;
            }
        }
    }
}
