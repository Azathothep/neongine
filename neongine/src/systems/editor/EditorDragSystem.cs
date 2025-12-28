using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using neon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace neongine.editor
{
    public class EditorDragSystem : IEditorUpdateSystem, IEditorDrawSystem
    {
        public bool ActiveInPlayMode => false;

        private Query<Point> m_Query = new Query<Point>(new IQueryFilter[]
        {
            new QueryFilter<IsDraggable>(FilterTerm.Has)
        });

        private float m_Radius = 4.0f;

        private float m_RotationAmount = 90.0f;

        private Point m_Hovered = null;

        private Point m_Dragged = null;

        private Vector3 m_Offset;

        private ButtonState m_PreviousLeftButtonState;

        private SpriteBatch m_SpriteBatch;

        private IEnumerable<(EntityID, Point)> m_QueryResult;

        public EditorDragSystem(SpriteBatch spriteBatch, float inputRadius)
        {
            m_SpriteBatch = spriteBatch;
            m_Radius = inputRadius;
        }

        public void Update(TimeSpan timeSpan)
        {
            m_QueryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            MouseState state = Mouse.GetState();
            Vector2 mousePosition = new Vector2(state.Position.X, state.Position.Y);

            bool leftButtonDown = m_PreviousLeftButtonState == ButtonState.Released && state.LeftButton == ButtonState.Pressed;
            bool leftButtonUp = m_PreviousLeftButtonState == ButtonState.Pressed && state.LeftButton == ButtonState.Released;

            m_PreviousLeftButtonState = state.LeftButton;

            if (m_Dragged != null)
            {
                if (leftButtonUp)
                {
                    m_Dragged = null;
                } else
                {
                    UpdateDraggedPoint((float)timeSpan.TotalSeconds, mousePosition);
                }

                return;
            } else
            {
                m_Hovered = GetClosestPoint(m_QueryResult, mousePosition);
            }

            if (leftButtonDown && m_Hovered != null)
            {
                m_Dragged = m_Hovered;
                m_Offset = mousePosition.ToVector3() - m_Dragged.WorldPosition;
            }
        }

        private void UpdateDraggedPoint(float deltaTime, Vector2 mousePosition)
        {
            m_Dragged.WorldPosition = mousePosition.ToVector3() - m_Offset;

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.E))
            {
                m_Dragged.WorldRotation += m_RotationAmount * deltaTime;
            } else if (keyboardState.IsKeyDown(Keys.A))
            {
                m_Dragged.WorldRotation -= m_RotationAmount * deltaTime;
            }
        }

        private Point GetClosestPoint(IEnumerable<(EntityID, Point)> draggables, Vector2 mousePosition)
        {
            float closestEntityDistance = m_Radius * m_Radius;
            Point closestPoint = null;

            foreach ((EntityID eid, Point p) in draggables)
            {
                Vector2 point2D = new Vector2(p.WorldPosition.X, p.WorldPosition.Y);
                float distanceToEntity = (mousePosition - point2D).LengthSquared();

                if (distanceToEntity < closestEntityDistance)
                {
                    closestEntityDistance = distanceToEntity;
                    closestPoint = p;
                }
            }

            return closestPoint;
        }

        public void Draw()
        {
            DrawPoints(m_QueryResult);
        }

        private void DrawPoints(IEnumerable<(EntityID, Point)> draggables)
        {
            m_SpriteBatch.Begin();

            foreach ((EntityID _, Point p) in draggables)
            {
                MonoGame.Primitives2D.DrawCircle(m_SpriteBatch,
                                        new Vector2(p.WorldPosition.X,
                                        p.WorldPosition.Y),
                                        m_Radius,
                                        8,
                                        p == m_Hovered ? Color.Green : Color.Red,
                                        8);
            }

            m_SpriteBatch.End();
        }
    }
}
