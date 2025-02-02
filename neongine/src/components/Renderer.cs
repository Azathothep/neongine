using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using neon;
using Newtonsoft.Json;

namespace neongine
{
	public class Renderer : IComponent
    {
        public EntityID EntityID { get; private set; }

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

        public IComponent Clone() => new Renderer(this);
    }
}
