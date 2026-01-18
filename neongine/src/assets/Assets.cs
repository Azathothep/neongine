using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    /// <summary>
    /// Manages assets loading using <c>ContentManager</c>
    /// </summary>
    public static class Assets
    {
        private static Dictionary<Type, Dictionary<string, object>> m_AssetDatabase;
        private static ContentManager m_ContentManager;

        public static void SetManager(ContentManager contentManager) => m_ContentManager = contentManager;

        /// <summary>
        /// Load an asset using the provided path
        /// </summary>
        public static T GetAsset<T>(string path)
        {
            return (T)GetAsset(path, typeof(T));
        }

        /// <summary>
        /// Load an asset using the provided path
        /// </summary>
        public static object GetAsset(string path, Type type)
        {
            object asset = GetAssetInDatabase(path, type);
            if (asset == null)
                asset = RegisterAssetInDatabse(path, type);

            return asset;
        }

        /// <summary>
        /// Get the provided asset's path. Only works if the asset has been previously loaded with <c>GetAsset</c>.
        /// </summary>
		public static string GetPath(object asset, Type type)
		{
			if (!m_AssetDatabase.TryGetValue(type, out Dictionary<string, object> database))
				return string.Empty;

			Dictionary<object, string> reverseDatabase = database.ToDictionary(d => d.Value, d => d.Key);

			if (!reverseDatabase.TryGetValue(asset, out string path))
				return string.Empty;

			return path;
		}

        private static object GetAssetInDatabase(string path, Type type)
        {
            if (m_AssetDatabase == null)
                m_AssetDatabase = new();

            if (!m_AssetDatabase.TryGetValue(type, out Dictionary<string, object> database))
                return null;

            if (!database.TryGetValue(path, out object asset))
                return null;

            return asset;
        }

        private static object RegisterAssetInDatabse(string path, Type type)
        {
            object asset = m_ContentManager.Load<object>(path);

            if (asset == null)
                return null;

            Dictionary<string, object> typeDatabase;

            if (!m_AssetDatabase.TryGetValue(type, out typeDatabase))
            {
                typeDatabase = new Dictionary<string, object>();
                m_AssetDatabase.Add(type, typeDatabase);
            }

            typeDatabase.Add(path, asset);

            return asset;
        }
    }
}
