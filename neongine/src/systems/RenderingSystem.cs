using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using neon;
using System.Diagnostics;
using System.Linq;
using System;

namespace neongine
{
    public class RenderingSystem : IDrawSystem
    {
        private static RenderingSystem instance;

        private SpriteBatch m_SpriteBatch;

        private Query<Renderer, Point> m_Query = new();

        public RenderingSystem(SpriteBatch spriteBatch)
        {
            m_SpriteBatch = spriteBatch;

            if (RenderingSystem.instance == null)
                RenderingSystem.instance = this;
        }

        public void Draw()
        {
            var qresult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            m_SpriteBatch.Begin();

            // Coordinates : BaseFactor, Zoom
            // Only draw things inside bounds with renderer's Texture2D.width / height (check overlap)
            // Check all places where move, use position, etc... Use Coordinates.From/ToPixels
            // Have a unique place for Drawing => static RenderingSystem.DrawCircle etc..., that converts position automatically

            foreach ((EntityID _, Renderer r, Point p) in qresult)
            {
                Render(r,
                        Coordinates.ToPixels(p.WorldPosition.X, p.WorldPosition.Y),
                        p.WorldRotation,
                        p.WorldScale);
            }

            m_SpriteBatch.End();
        }

        private void Render(Renderer renderer, Vector2 position, float rotation, Vector2 scale)
        {
            m_SpriteBatch.Draw(renderer.Texture,
                             position * Camera.Main.Factor,
                             null,
                            renderer.Color,
                            rotation * (float)(Math.PI / 180f),
                            new Vector2(renderer.Texture.Width / 2 * renderer.Scale, renderer.Texture.Height / 2 * renderer.Scale) * renderer.Scale,
                            Vector2.One * scale * renderer.Scale * Camera.Main.Factor,
                            SpriteEffects.None,
                            0f);
        }

        public static void DrawCircle(Vector2 p, float radius, Color color)
        {
            instance.m_SpriteBatch.Begin();

            MonoGame.Primitives2D.DrawCircle(instance.m_SpriteBatch,
                Coordinates.ToPixels(p) * Camera.Main.Factor,
                Coordinates.ToPixels(radius) * Camera.Main.Factor,
                16,
                color);

            instance.m_SpriteBatch.End();
        }

        public static void DrawPolygon(Vector2 p, Vector2[] vertices, Color color)
        {
            instance.m_SpriteBatch.Begin();

            for (int i = 0; i < vertices.Length - 1; i++) {
                Vector2 startPosition = Coordinates.ToPixels(p + vertices[i]) * Camera.Main.Factor;
                Vector2 endPosition = Coordinates.ToPixels(p + vertices[i + 1]) * Camera.Main.Factor;
                MonoGame.Primitives2D.DrawLine(instance.m_SpriteBatch, startPosition, endPosition, color);
            }

            {
                Vector2 startPosition = Coordinates.ToPixels(p + vertices[vertices.Length - 1]) * Camera.Main.Factor;
                Vector2 endPosition = Coordinates.ToPixels(p + vertices[0]) * Camera.Main.Factor;
                MonoGame.Primitives2D.DrawLine(instance.m_SpriteBatch, startPosition, endPosition, color);
            }

            instance.m_SpriteBatch.End();
        }

        public static void DrawBounds(Point p, Bounds bounds)
        {
            instance.m_SpriteBatch.Begin();

            MonoGame.Primitives2D.DrawRectangle(instance.m_SpriteBatch,
            new Rectangle((int)(Coordinates.ToPixels(p.WorldPosition.X + bounds.X) * Camera.Main.Factor),
                        (int)(Coordinates.ToPixels(p.WorldPosition.Y + bounds.Y) * Camera.Main.Factor),
                        (int)(Coordinates.ToPixels(bounds.Width) * Camera.Main.Factor),
                        (int)(Coordinates.ToPixels(bounds.Height) * Camera.Main.Factor)),
            0.0f,
            Color.Blue);

            instance.m_SpriteBatch.End();
        }
    }
}
