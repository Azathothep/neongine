using neon;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;

namespace neongine
{
    public static class Serializer
    {

        private static ISerializer m_Serializer;

        public static void SetSerializer(ISerializer serializer) => m_Serializer = serializer;

        public static string SerializeScene(SceneDefinition sceneDefinition) => m_Serializer.SerializeScene(sceneDefinition);

        public static SceneDefinition DeserializeScene(string serializedData) => m_Serializer.DeserializeScene(serializedData);

        public static string SerializeComponent<T>(T component) where T : Component => m_Serializer.SerializeComponent(component);

        public static Component DeserializeComponent(string serializedData, Type componentType) => m_Serializer.DeserializeComponent(serializedData, componentType);

        public static string SerializeSystem<T>(T system) where T : IGameSystem => m_Serializer.SerializeSystem(system);

        public static IGameSystem DeserializeSystem(string serializedData, Type systemType) => m_Serializer.DeserializeSystem(serializedData, systemType);

        public static string GetMemberValue(string serializedData, string memberName) => m_Serializer.GetMemberValue(serializedData, memberName);
    }
}
