using neon;
using System;

namespace neongine
{
    /// <summary>
    /// Contains a list of the all the currently loaded game's sytems and its entities
    /// </summary>
    public struct RuntimeScene
    {
        public ISystem[] GameSystems;

        public EntityID[] Roots;
    }

    /// <summary>
    /// Defines a ready-to-be-serialized scene in terms of <c>SystemData</c> and <c>EntityData</c>
    /// </summary>
    public struct SceneDefinition
    {
        public SystemData[] SystemDatas { get; set; }

        /// <summary>
        /// The loaded systems's HashCodes. Some systems may be referenced by other systems without being loaded yet, this value is used to differentiate them. 
        /// </summary>
        public uint[] LoadedSystems { get; set; }
        public EntityGraph EntityGraph { get; set; }
    }

    /// <summary>
    /// Stores the scene's root <c>EntityData</c>
    /// </summary>
    public struct EntityGraph
    {
        /// <summary>
        /// The data of the scene's root entities
        /// </summary>
        public EntityData[] entities;
    }

    /// <summary>
    /// Stores ready-to-be-serialized datas about an entity
    /// </summary>
    public struct EntityData
    {
        /// <summary>
        /// The entity id in the scene. Used to resolve references of entities in components and systems when rebuilding the scene.
        /// </summary>
        public uint id;

        /// <summary>
        /// The active state of the entity
        /// </summary>
        public bool active;

        /// <summary>
        /// The datas for every components this entity is owning.
        /// </summary>
        public ComponentData[] components; 

        /// <summary>
        /// The datas of the entity's children.
        /// </summary>
        public EntityData[] children;
    }

    /// <summary>
    /// Stores ready-to-be-serialized datas about a component
    /// </summary>
    public struct ComponentData
    {
        /// <summary>
        /// The entity id of this component in the scene. Used to resolve references of components in other components and systems when rebuilding the scene.
        /// </summary>
        public uint id;

        /// <summary>
        /// The active state of the component
        /// </summary>
        public bool active;

        /// <summary>
        /// The component's type name, in AssemblyQualifiedName format
        /// </summary>
        public string typeName;

        /// <summary>
        /// The serialized datas of the component
        /// </summary>
        public string serializedData;
    }

    /// <summary>
    /// Stores ready-to-be-serialized datas about a system
    /// </summary>
    public struct SystemData
    {
        /// <summary>
        /// The system's hashcode. Used to resolve references of systems in components and other systems when rebuilding the scene.
        /// </summary>
        public uint id;

        /// <summary>
        /// The system's type name, in AssemblyQualifiedName format
        /// </summary>
        public string typeName;

        /// <summary>
        /// The serialized datas of the system
        /// </summary>
        public string serializedData;
    }
}
