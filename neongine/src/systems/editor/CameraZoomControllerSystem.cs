using System;
using Microsoft.Xna.Framework.Input;
using neon;

namespace neongine
{
    public class CameraZoomControllerSystem : IUpdateSystem
    {
        private float m_Speed;

        public CameraZoomControllerSystem(float speed)
        {
            m_Speed = speed;
        }

        public void Update(TimeSpan timeSpan)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            float ratio = (m_Speed - 1.0f) * (float)timeSpan.TotalSeconds;

            if (keyboardState.IsKeyDown(Keys.O))
                Camera.Main.Zoom *= 1 + ratio;
            else if (keyboardState.IsKeyDown(Keys.I))
                Camera.Main.Zoom *= 1 - ratio;
        }
    }
}

