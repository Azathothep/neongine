using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using neon;
using Newtonsoft.Json;

namespace neongine
{
    /// <summary>
    /// Add a sprite to the entity
    /// </summary>
	public class Renderer : Component
    {
        /// <summary>
        /// The sprite's texture. Use <c>Assets.GetAsset-Texture2D-(string)</c> to load a texture included through the MonoGame Content Builder.
        /// </summary>
		[Serialize]
        public Texture2D Texture { get; private set; }

        public Color Color { get; set; } = Color.White;

        [Serialize]
        public float Scale { get; private set; } = 1.0f;

		private Renderer() { }

        public Renderer(Texture2D texture, float scale = 1.0f)
        {
            this.Texture = texture;
            this.Scale = scale;
        }

        public Renderer(Renderer other) : this(other.Texture, other.Scale)
        {

        }

        public override Component Clone() => new Renderer(this);
    }
}
