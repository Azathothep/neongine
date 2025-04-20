using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using neon;

namespace neongine {
    [Serialize, AllowMultiple, Order(OrderType.Before, typeof(CollisionSystem)), Order(OrderType.Before, typeof(VelocitySystem))]
    public class ManualVelocityControlSystem : IUpdateSystem
    {
        [Serialize]
        private Velocity m_Velocity;

        [Serialize]
        private float m_Speed = 1;

        private ManualVelocityControlSystem() {}

        public ManualVelocityControlSystem(Velocity velocity, float speed = 1.0f) {
            m_Velocity = velocity;
            m_Speed = speed;
        }

        public void Update(TimeSpan timeSpan)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Vector2 input = new Vector2();

            if (keyboardState.IsKeyDown(Keys.Q)) {
                input.X = -1;
            } else if (keyboardState.IsKeyDown(Keys.D)) {
                input.X = 1;
            }

            if (keyboardState.IsKeyDown(Keys.Z)) {
                input.Y = -1;
            } else if (keyboardState.IsKeyDown(Keys.S)) {
                input.Y = 1;
            }

            if (input != Vector2.Zero) input.Normalize();

            m_Velocity.Value = input * m_Speed;
        }
    }
}

