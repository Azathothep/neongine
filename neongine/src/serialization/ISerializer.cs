using neon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public interface ISerializer
    {
        public string SerializeScene(SceneDefinition sceneDefinition);

        public SceneDefinition DeserializeScene(string serializedData);

        public string SerializeComponent<T>(T component) where T : Component;
        public Component DeserializeComponent(string serializedData, Type componentType);

        public string SerializeSystem<T>(T system) where T : IGameSystem;
        public IGameSystem DeserializeSystem(string serializedData, Type systemType);

        public string GetMemberValue(string serializedData, string memberName);
    }
}
