using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using neon;

namespace neongine.editor
{
    [Order(OrderType.After, typeof(RenderingSystem))]
    public class EditorColliderVisualizer : IEditorUpdateSystem, IEditorDrawSystem
    {
        public bool ActiveInPlayMode => true;

        private Query<Point, Collider> m_Query = new Query<Point, Collider>();
        private IEnumerable<(EntityID, Point, Collider)> m_QueryResult;

        public void Update(TimeSpan timeSpan)
        {
            m_QueryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Safe);

            foreach ((EntityID _, Point p, Collider c) in m_QueryResult)
            {
                c.UpdateShape(p.WorldRotation, p.WorldScale);
            }
        }

        public void Draw()
        {
            foreach ((EntityID id, Point p, Collider c) in m_QueryResult)
            {
                Color color = CollisionSystem.IsColliding(id) ? Color.Red : Color.Yellow;

                if (c.BaseShape.IsPolygon)
                    RenderingSystem.DrawPolygon(p.WorldPosition2D, c.Shape.Vertices, color);
                else
                    RenderingSystem.DrawCircle(p.WorldPosition2D, c.Shape.Radius, 8, color);

                //RenderingSystem.DrawBounds(p.WorldPosition2D, c.Bound);
            }
        }
    }
}
