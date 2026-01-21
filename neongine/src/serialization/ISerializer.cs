using neon;
using System;

namespace neongine
{
    /// <summary>
    /// Interface for implementing a serializer
    /// </summary>
    public interface ISerializer
    {
        public string SerializeScene(SceneDefinition sceneDefinition);

        public SceneDefinition DeserializeScene(string serializedData);

        public string SerializeComponent<T>(T component) where T : Component;
        public Component DeserializeComponent(string serializedData, Type componentType);

        public string SerializeSystem<T>(T system) where T : ISystem;
        public ISystem DeserializeSystem(string serializedData, Type systemType);

        /// <summary>
        /// Get the value of the taget member in the provided serialized object
        /// </summary>
        public string GetMemberValue(string serializedData, string memberName);
    }
}
