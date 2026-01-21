using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using neon;
using System;
using System.Collections.Generic;

namespace neongine.editor
{
    /// <summary>
    /// Add the ability to move the entities using the mouse when in Editor
    /// </summary>
    public class EditorDragSystem : IEditorUpdateSystem, IEditorDrawSystem
    {
        public bool ActiveInPlayMode => false;

        private Query<Transform> m_Query = new Query<Transform>( [
            new QueryFilter<NotDraggable>(FilterTerm.HasNot)
        ]);

        private float m_Radius = 4.0f;

        private float m_RotationAmount = 90.0f;

        private Transform m_Hovered = null;

        private Transform m_Dragged = null;

        private Vector3 m_Offset;

        private ButtonState m_PreviousLeftButtonState;

        private IEnumerable<(EntityID, Transform)> m_QueryResult;

        public EditorDragSystem(float inputRadius)
        {
            m_Radius = inputRadius;
        }

        public void Update(TimeSpan timeSpan)
        {
            m_QueryResult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            MouseState state = Mouse.GetState();
            Vector2 mousePosition = Camera.Main.ScreenToWorld(state.Position.X, state.Position.Y);

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

        private Transform GetClosestPoint(IEnumerable<(EntityID, Transform)> draggables, Vector2 mousePosition)
        {
            float closestEntityDistance = m_Radius * m_Radius;
            Transform closestTransform = null;

            foreach ((EntityID eid, Transform t) in draggables)
            {
                Vector2 point2D = new Vector2(t.WorldPosition.X, t.WorldPosition.Y);
                float distanceToEntity = (mousePosition - point2D).LengthSquared();

                if (distanceToEntity < closestEntityDistance)
                {
                    closestEntityDistance = distanceToEntity;
                    closestTransform = t;
                }
            }

            return closestTransform;
        }

        public void Draw()
        {
            DrawPoints(m_QueryResult);
        }

        private void DrawPoints(IEnumerable<(EntityID, Transform)> entities)
        {
            foreach ((EntityID _, Transform t) in entities)
            {
                RenderingSystem.DrawCircle(new Vector2(t.WorldPosition.X,
                                        t.WorldPosition.Y),
                                        m_Radius,
                                        4,
                                        t == m_Hovered ? Color.Green : Color.Red,
                                        3.0f);
            }
        }
    }
}
