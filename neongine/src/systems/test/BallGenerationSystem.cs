using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using neon;
using System;
using System.Diagnostics;

namespace neongine
{
    [Serialize]
    public class BallGenerationSystem : IUpdateSystem
    {
        private bool m_KeyPressed = false;
        
        private Texture2D m_Texture;

        [Serialize]
        private Rect m_Bounds;

        private Random m_Random = new Random();

        public BallGenerationSystem(Rect generationBounds)
        {
            m_Texture = Assets.GetAsset<Texture2D>("ball");
            m_Bounds = generationBounds;
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
            EntityID entity = Neongine.Entity();
            Point point = entity.Get<Point>();
            point.WorldPosition = GetRandomPosition();
            point.WorldScale = Vector2.One * 0.5f;

            entity.Add<Ball>();
            entity.Add(new Renderer(m_Texture));
            entity.Add(new Velocity(GetRandomDirection()));
            entity.Add(new AngleVelocity(m_Random.Next(1, 3)));
            entity.Add(new Collider(new Geometry(GeometryType.Rectangle, 60)));

            // Debug.WriteLine("New ball generated at " + point.WorldPosition);
        }

        private Vector3 GetRandomPosition() {
            float x = m_Random.Next((int)m_Bounds.Width);
            float y = m_Random.Next((int)m_Bounds.Height);

            return new Vector3(m_Bounds.X + x, m_Bounds.Y + y, 0);
        }

        private Vector2 GetRandomDirection() {
            float angle = float.DegreesToRadians(m_Random.Next(0, 360));

            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private EntityID Copy(EntityID entity) {
            EntityID entityCopy = Entities.GetID();
            
            EntityID[] children = entity.GetChildren(false);

            foreach (var child in children) {
                EntityID childCopy = Copy(child);
                childCopy.SetParent(entityCopy);
            }

            IComponent[] components = entity.GetAll();

            foreach (var component in components) {
                Components.Add(entity, component, component.GetType());
            }

            return entity;
        }
    }
}
