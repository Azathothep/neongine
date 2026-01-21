using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    /// <summary>
    /// Automatically rotates entities with an <c>AngleVelocity</c> component
    /// </summary>
    [Order(OrderType.After, typeof(CollisionSystem))]
    public class AngleVelocitySystem : IGameUpdateSystem
    {
        private Query<AngleVelocity, Transform> m_Query;

        public AngleVelocitySystem()
        {
            m_Query = new Query<AngleVelocity, Transform>(new IQueryFilter[]
            {
                new QueryFilter<Collider>(FilterTerm.HasNot) // CollisionSystem doesn't support AngleVelocitied entities
            });
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, AngleVelocity, Transform)> qResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            foreach ((EntityID _, AngleVelocity v, Transform t) in qResult)
            {
                t.WorldRotation += v.Value * (float)timeSpan.TotalSeconds;
            }
        }
    }
}
