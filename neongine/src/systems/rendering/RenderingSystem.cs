using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using neon;
using System;
using neongine.editor;

namespace neongine
{
    public class RenderingSystem : IEditorDrawSystem, IGameDrawSystem
    {
        public bool ActiveInPlayMode => true;

        private static RenderingSystem instance;

        private SpriteBatch m_SpriteBatch;
        private SpriteFont m_BaseFont;

        private Query<Renderer, Point> m_Query = new();

        public RenderingSystem(SpriteBatch spriteBatch, SpriteFont baseFont)
        {
            m_SpriteBatch = spriteBatch;
            m_BaseFont = baseFont;

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
                        Camera.Main.WorldToScreen(p.WorldPosition.X, p.WorldPosition.Y),
                        p.WorldRotation,
                        p.WorldScale);
            }

            m_SpriteBatch.End();
        }

        private void Render(Renderer renderer, Vector2 position, float rotation, Vector2 scale)
        {
            m_SpriteBatch.Draw(renderer.Texture,
                             position,
                             null,
                            renderer.Color,
                            rotation * (float)(Math.PI / 180f),
                            new Vector2(renderer.Texture.Width / 2 * renderer.Scale, renderer.Texture.Height / 2 * renderer.Scale) * renderer.Scale,
                            Vector2.One * scale * renderer.Scale * Camera.Main.Zoom,
                            SpriteEffects.None,
                            0f);
        }

        public static void DrawCircle(Vector2 p, float radius, int resolution, Color color, float thickness = 1.0f)
        {
            instance.m_SpriteBatch.Begin();

            MonoGame.Primitives2D.DrawCircle(instance.m_SpriteBatch,
                Camera.Main.WorldToScreen(p),
                Camera.Main.WorldToScreen(radius),
                resolution,
                color,
                thickness);

            instance.m_SpriteBatch.End();
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            instance.m_SpriteBatch.Begin();

            Vector2 startPosition = Camera.Main.WorldToScreen(start);
            Vector2 endPosition = Camera.Main.WorldToScreen(end);
            MonoGame.Primitives2D.DrawLine(instance.m_SpriteBatch, startPosition, endPosition, color);

            instance.m_SpriteBatch.End();
        }

        public static void DrawRectangle(Rectangle rectangle, float rotation, Color color)
        {
            instance.m_SpriteBatch.Begin();

            MonoGame.Primitives2D.DrawRectangle(instance.m_SpriteBatch, rectangle, rotation, color);

            instance.m_SpriteBatch.End();
        }

        public static void DrawPolygon(Vector2 p, Vector2[] vertices, Color color)
        {
            instance.m_SpriteBatch.Begin();

            for (int i = 0; i < vertices.Length - 1; i++) {
                Vector2 startPosition = Camera.Main.WorldToScreen(p + vertices[i]);
                Vector2 endPosition = Camera.Main.WorldToScreen(p + vertices[i + 1]);
                MonoGame.Primitives2D.DrawLine(instance.m_SpriteBatch, startPosition, endPosition, color);
            }

            {
                Vector2 startPosition = Camera.Main.WorldToScreen(p + vertices[vertices.Length - 1]);
                Vector2 endPosition = Camera.Main.WorldToScreen(p + vertices[0]);
                MonoGame.Primitives2D.DrawLine(instance.m_SpriteBatch, startPosition, endPosition, color);
            }

            instance.m_SpriteBatch.End();
        }

        public static void DrawBounds(Vector2 position, Bounds bounds)
        {
            instance.m_SpriteBatch.Begin();

            Vector2 screenPosition = Camera.Main.WorldToScreen(position.X + bounds.X, position.Y + bounds.Y);

            MonoGame.Primitives2D.DrawRectangle(instance.m_SpriteBatch,
            new Rectangle((int)screenPosition.X,
                        (int)screenPosition.Y,
                        (int)Camera.Main.WorldToScreen(bounds.Width),
                        (int)Camera.Main.WorldToScreen(bounds.Height)),
            0.0f,
            Color.Blue);

            instance.m_SpriteBatch.End();
        }

        public static void DrawText(string text, Vector2 screenPosition, int size, Color color)
        {
            instance.m_SpriteBatch.Begin();

            instance.m_SpriteBatch.DrawString(instance.m_BaseFont, text, screenPosition, color, 0, Vector2.Zero, size, SpriteEffects.None, 0.5f);

            instance.m_SpriteBatch.End();
        }
    }
}
