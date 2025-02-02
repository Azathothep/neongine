using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using neon;
using System.Diagnostics;
using System.Linq;
using System;

namespace neongine
{
    [DoNotSerialize]
    public class RenderingSystem : IDrawSystem
    {
        private SpriteBatch m_SpriteBatch;

        private Query<Renderer, Point> m_Query = new();

        public RenderingSystem(SpriteBatch spriteBatch)
        {
            m_SpriteBatch = spriteBatch;
        }

        public void Draw()
        {
            var qresult = QueryBuilder.Get(m_Query, QueryType.Cached, QueryResultMode.Unsafe);

            m_SpriteBatch.Begin();

            foreach ((EntityID _, Renderer r, Point p) in qresult)
            {
                Render(r,
                        new Vector2(p.WorldPosition.X, p.WorldPosition.Y),
                        p.WorldRotation,
                        p.WorldScale
                        );
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
                            Vector2.One * scale * renderer.Scale,
                            SpriteEffects.None,
                            0f);
        }
    }
}
