using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    [Serialize, Order(OrderType.After, typeof(CollisionSystem))]
    public class AngleVelocitySystem : IGameUpdateSystem
    {
        private Query<AngleVelocity, Point> m_Query;

        public AngleVelocitySystem()
        {
            m_Query = new Query<AngleVelocity, Point>(new IQueryFilter[]
            {
                new QueryFilter<Collider>(FilterTerm.HasNot) // CollisionSystem doesn't support AngleVelocitied entities
            });
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, AngleVelocity, Point)> qResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            foreach ((EntityID _, AngleVelocity v, Point r) in qResult)
            {
                r.WorldRotation += v.Value * (float)timeSpan.TotalSeconds;
            }
        }
    }
}
