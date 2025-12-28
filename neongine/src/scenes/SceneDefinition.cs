using neon;
using System;

namespace neongine
{
    public struct RuntimeScene
    {
        public ISystem[] GameSystems;

        public EntityID[] Roots;
    }

    public struct SceneDefinition
    {
        public SystemData[] SystemDatas { get; set; }

        public EntityGraph EntityGraph { get; set; }
    }

    public struct EntityGraph
    {
        public EntityData[] entities;
    }

    public struct EntityData
    {
        public uint id;
        public bool active;
        public ComponentData[] components; 
        public EntityData[] children;
    }

    public struct ComponentData
    {
        public uint id;
        public bool active;
        public string typeName;
        public string serializedData;
    }

    public struct SystemData
    {
        public string typeName;
        public string serializedData;
    }
}
