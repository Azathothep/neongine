using neon;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace neongine
{
    /// <summary>
    /// Provides a way to interact with the underlying serializer implementation
    /// </summary>
    public static class Serializer
    {

        private static ISerializer m_Serializer;

        public static void SetSerializer(ISerializer serializer) => m_Serializer = serializer;

        public static string SerializeScene(SceneDefinition sceneDefinition) => m_Serializer.SerializeScene(sceneDefinition);

        public static SceneDefinition DeserializeScene(string serializedData) => m_Serializer.DeserializeScene(serializedData);

        public static string SerializeComponent<T>(T component) where T : Component => m_Serializer.SerializeComponent(component);

        public static Component DeserializeComponent(string serializedData, Type componentType) => m_Serializer.DeserializeComponent(serializedData, componentType);

        public static string SerializeSystem<T>(T system) where T : ISystem => m_Serializer.SerializeSystem(system);

        public static ISystem DeserializeSystem(string serializedData, Type systemType) => m_Serializer.DeserializeSystem(serializedData, systemType);

        /// <summary>
        /// Get the value of the taget member in the provided serialized object
        /// </summary>
        public static string GetMemberValue(string serializedData, string memberName) => m_Serializer.GetMemberValue(serializedData, memberName);
    }
}
