using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using neon;

namespace neongine {
    [Serialize]
    public class BouncingBallSystem : IGameUpdateSystem
    {
        private Query<Transform, Velocity> m_Query = new Query<Transform, Velocity>([
            new QueryFilter<Ball>(FilterTerm.Has)
        ]);

        [Serialize]
        private Rect m_Bounds;

        public BouncingBallSystem(Rect bounds) {
            m_Bounds = bounds;
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, Transform, Velocity)> queryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);
        
            foreach ((EntityID _, Transform t, Velocity v) in queryResult) {
                if (t.WorldPosition.X < m_Bounds.X || t.WorldPosition.X > m_Bounds.X + m_Bounds.Width)
                    v.Value.X = -v.Value.X;

                if (t.WorldPosition.Y < m_Bounds.Y || t.WorldPosition.Y > m_Bounds.Y + m_Bounds.Height)
                    v.Value.Y = -v.Value.Y;
            }
        }
    }
}

