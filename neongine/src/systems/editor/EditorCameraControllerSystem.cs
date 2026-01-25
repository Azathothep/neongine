using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using neon;

namespace neongine.editor
{
    /// <summary>
    /// Controls the camera when in Editor.
    /// Use Arrow keys to move and I/O keys to zoom in / out.
    /// </summary>
    public class EditorCameraControllerSystem : IEditorUpdateSystem
    {
        public bool ActiveInPlayMode => false;

        private float m_MoveSpeed;
        private float m_ZoomSpeed;

        public EditorCameraControllerSystem(float moveSpeed, float zoomSpeed)
        {
            m_MoveSpeed = moveSpeed;
            m_ZoomSpeed = zoomSpeed;
        }

        public void Update(TimeSpan timeSpan)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            float ratio = (m_ZoomSpeed - 1.0f) * (float)timeSpan.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.O))
                Camera.Main.Zoom *= 1 + ratio;
            else if (keyboardState.IsKeyDown(Keys.I))
                Camera.Main.Zoom *= 1 - ratio;

            Vector2 input = new Vector2();

            if (keyboardState.IsKeyDown(Keys.A)) {
                input.X = -1;
            } else if (keyboardState.IsKeyDown(Keys.D)) {
                input.X = 1;
            }

            if (keyboardState.IsKeyDown(Keys.W)) {
                input.Y = 1;
            } else if (keyboardState.IsKeyDown(Keys.S)) {
                input.Y = -1;
            }

            if (input != Vector2.Zero) input.Normalize();

            Vector2 translation = input * m_MoveSpeed * (float)timeSpan.TotalSeconds;
            Camera.Main.Transform.WorldPosition += new Vector3(translation.X, translation.Y, 0);
        }
    }
}

