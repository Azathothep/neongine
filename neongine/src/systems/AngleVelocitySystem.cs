using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    [Serialize]
    public class AngleVelocitySystem : IUpdateSystem
    {
        private Query<AngleVelocity, Point> m_Query;

        public AngleVelocitySystem()
        {
            m_Query = new Query<AngleVelocity, Point>(new IQueryFilter[]
            {

            });
        }

        public void Update(TimeSpan timeSpan)
        {
            IEnumerable<(EntityID, AngleVelocity, Point)> qResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            foreach ((EntityID _, AngleVelocity v, Point r) in qResult)
            {
                r.WorldRotation += v.Value;
            }
        }
    }
}
