using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public static class AssetConverters
    {
        private static JsonConverter[] m_Converters = new JsonConverter[]
        {
            new AssetConverter<Texture2D>()
        };

        public static JsonConverter[] Converters => m_Converters;
    }
}
