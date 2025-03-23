using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using neon;
using System;

namespace neongine
{
    public class BallGenerationSystem : IUpdateSystem
    {
        private bool m_KeyPressed = false;

        private Texture2D m_Texture;

        public BallGenerationSystem(Texture2D texture)
        {
            m_Texture = texture;
        }

        public void Update(TimeSpan timeSpan)
        {
            if (!m_KeyPressed && Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                GenerateEntity();
                m_KeyPressed = true;
            }
            else if (m_KeyPressed && Keyboard.GetState().IsKeyUp(Keys.Space))
                m_KeyPressed = false;
        }

        private void GenerateEntity()
        {
            EntityID id = Neongine.Entity("ball");

            id.Add(new Renderer(m_Texture));
            //id.Add(new Collider(new Shape(Shape.Type.Rectangle, 1, 1)));
        }
    }
}
