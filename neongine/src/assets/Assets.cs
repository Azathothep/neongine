using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public static class Assets
    {
        private static Dictionary<Type, Dictionary<string, object>> m_AssetDatabase;
        private static ContentManager m_ContentManager;

        public static void SetManager(ContentManager contentManager) => m_ContentManager = contentManager;

        public static T GetAsset<T>(string name)
        {
            return (T)GetAsset(name, typeof(T));
        }

        public static object GetAsset(string name, Type type)
        {
            object asset = GetAssetInDatabase(name, type);
            if (asset == null)
                asset = RegisterAssetInDatabse(name, type);

            return asset;
        }

		public static string GetName(object asset, Type type)
		{
			if (!m_AssetDatabase.TryGetValue(type, out Dictionary<string, object> database))
				return string.Empty;

			Dictionary<object, string> reverseDatabase = database.ToDictionary(d => d.Value, d => d.Key);

			if (!reverseDatabase.TryGetValue(asset, out string name))
				return string.Empty;

			return name;
		}

        private static object GetAssetInDatabase(string name, Type type)
        {
            if (m_AssetDatabase == null)
                m_AssetDatabase = new();

            if (!m_AssetDatabase.TryGetValue(type, out Dictionary<string, object> database))
                return null;

            if (!database.TryGetValue(name, out object asset))
                return null;

            return asset;
        }

        private static object RegisterAssetInDatabse(string name, Type type)
        {
            object asset = m_ContentManager.Load<object>(name);

            if (asset == null)
                return null;

            Dictionary<string, object> typeDatabase;

            if (!m_AssetDatabase.TryGetValue(type, out typeDatabase))
            {
                typeDatabase = new Dictionary<string, object>();
                m_AssetDatabase.Add(type, typeDatabase);
            }

            typeDatabase.Add(name, asset);

            return asset;
        }
    }
}
