using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace neongine
{
    /// <summary>
    /// Specify which types must use the <c>AssetConverter</c>
    /// </summary>
    public static class AssetConverters
    {
        private static JsonConverter[] m_Converters = new JsonConverter[]
        {
            new AssetConverter<Texture2D>(),
            new AssetConverter<SpriteFont>()
        };

        public static JsonConverter[] Converters => m_Converters;
    }
}
