using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using neon;

namespace neongine {
    [Serialize]
    public class BouncingBallSystem : IUpdateSystem
    {
        private Query<Point, Velocity> m_Query = new Query<Point, Velocity>([
            new QueryFilter<Ball>(FilterTerm.Has)
        ]);

        [Serialize]
        private Rect m_Bounds;

        public BouncingBallSystem(Rect bounds) {
            m_Bounds = bounds;
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, Point, Velocity)> queryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);
        
            foreach ((EntityID _, Point p, Velocity v) in queryResult) {
                if (p.WorldPosition.X < m_Bounds.X || p.WorldPosition.X > m_Bounds.X + m_Bounds.Width)
                    v.Value.X = -v.Value.X;

                if (p.WorldPosition.Y < m_Bounds.Y || p.WorldPosition.Y > m_Bounds.Y + m_Bounds.Height)
                    v.Value.Y = -v.Value.Y;
            }
        }
    }
}

