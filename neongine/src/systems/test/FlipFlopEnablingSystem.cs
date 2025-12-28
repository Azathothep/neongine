using Microsoft.Xna.Framework.Input;
using neon;
using System;

namespace neongine
{
    [Serialize]
    public class FlipFlopEnablingSystem : IGameUpdateSystem
    {
        private bool m_KeyPressed = false;

        private EntityID m_Entity;

        public FlipFlopEnablingSystem(EntityID entityID)
        {
            m_Entity = entityID;
        }

        public void Update(TimeSpan timeSpan)
        {
            if (!m_KeyPressed && Keyboard.GetState().IsKeyDown(Keys.Space))
            {
            m_Entity.active = !m_Entity.active;
                m_KeyPressed = true;
            }
            else if (m_KeyPressed && Keyboard.GetState().IsKeyUp(Keys.Space))
                m_KeyPressed = false;
        }
    }
}
