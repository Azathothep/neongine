using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using neon;

namespace neongine.editor
{
    /// <summary>
    /// Shows the colliders and their collision / trigger state when in Editor
    /// </summary>
    [Order(OrderType.After, typeof(RenderingSystem))]
    public class EditorColliderVisualizer : IEditorUpdateSystem, IEditorDrawSystem
    {
        public bool ActiveInPlayMode => true;

        private Query<Transform, Collider> m_Query = new Query<Transform, Collider>();
        private IEnumerable<(EntityID, Transform, Collider)> m_QueryResult;

        public void Update(TimeSpan timeSpan)
        {
            m_QueryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Safe);

            foreach ((EntityID _, Transform t, Collider c) in m_QueryResult)
            {
                c.UpdateShape(t.WorldRotation, t.WorldScale);
            }
        }

        public void Draw()
        {
            foreach ((EntityID id, Transform t, Collider c) in m_QueryResult)
            {
                Color color = CollisionSystem.IsColliding(id) ? Color.Red : Color.Yellow;

                if (c.BaseShape.IsPolygon)
                    RenderingSystem.DrawPolygon(t.WorldPosition2D, c.Shape.Vertices, color);
                else
                    RenderingSystem.DrawCircle(t.WorldPosition2D, c.Shape.Radius, 8, color);

                //RenderingSystem.DrawBounds(p.WorldPosition2D, c.Bound);
            }
        }
    }
}
