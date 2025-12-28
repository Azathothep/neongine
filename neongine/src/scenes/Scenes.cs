using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using neon;

namespace neongine
{
    public static class Scenes
    {
		private struct SceneConstructor
		{
            public Dictionary<uint, EntityID> SceneIDToEntityID = new();
			public Dictionary<Type, MemberInfo[]> TypeToDependentMembers = new();
            public Dictionary<object, uint[]> Dependencies = new();

            public SceneConstructor() { }
        }

        public static void Load(string scenePath)
        {
            string jsonString = System.IO.File.ReadAllText(scenePath);

            Debug.WriteLine("Loading scene " + scenePath);

            SceneDefinition sceneDefinition = Serializer.DeserializeScene(jsonString);

            Load(sceneDefinition);
        }

        public static void Load(SceneDefinition scene)
        {
            SceneConstructor sceneConstructor = new();

            BuildHierarchy(scene.EntityGraph, sceneConstructor);

            BuildSystems(scene.SystemDatas, sceneConstructor);

            ResolveSceneDependencies(sceneConstructor);

            // EntityID[] rootIds = Entities.GetRoots();
            // IGameSystem[] gameSystems = Systems.GetLoadedSystems();
        }

        public static void Unload(RuntimeScene scene, bool onlySerializable = true)
        {
            ISystem[] serializableSystems = onlySerializable ? GetSerializableSystems(scene.GameSystems) : scene.GameSystems;

            foreach (var system in serializableSystems)
                Systems.Remove(system);

            EntityID[] rootEntities = Entities.GetRoots();

            foreach (var entity in rootEntities)
                Entities.Destroy(entity);
        }

        public static RuntimeScene GetRuntime()
        {
            RuntimeScene scene = new RuntimeScene();

            scene.Roots = Entities.GetRoots();

            scene.GameSystems = Systems.GetLoadedGameSystems();

            return scene;
        }

        private static void BuildHierarchy(EntityGraph entityGraph, SceneConstructor sceneConstructor)
        {
			int count = entityGraph.entities.Length;

            EntityID[] rootEntities = new EntityID[count];

			for (int i = 0; i < count; i++)
			{
				rootEntities[i] = BuildEntity(entityGraph.entities[i], sceneConstructor);
			}
        }

        private static EntityID BuildEntity(EntityData entityData, SceneConstructor sceneConstructor)
        {
            EntityID entityID = Entities.GetID();

            sceneConstructor.SceneIDToEntityID.Add(entityData.id, entityID);

            foreach (var componentData in entityData.components)
			{
                Component component = BuildComponent(entityID, componentData);
				sceneConstructor.SceneIDToEntityID.Add(componentData.id, component.EntityID);

                RegisterDependencies(component, componentData.serializedData, sceneConstructor);
			}

            foreach (var childData in entityData.children)
            {
                EntityID childID = BuildEntity(childData, sceneConstructor);
                childID.SetParent(entityID);
            }

            entityID.active = entityData.active;

            return entityID;
        }

        private static Component BuildComponent(EntityID entityID, ComponentData componentData)
        {
            Type componentType = Type.GetType(componentData.typeName);

            Component component = Serializer.DeserializeComponent(componentData.serializedData, componentType);

            Component newComponent = Components.Add(entityID, component, componentType);

            newComponent.EntityID.active = componentData.active;

			return newComponent;
        }

        private static void RegisterDependencies(object existingObject, string serializedData, SceneConstructor sceneConstructor)
        {
            Type type = existingObject.GetType();
            MemberInfo[] memberDependencies;

            if (!sceneConstructor.TypeToDependentMembers.TryGetValue(type, out memberDependencies))
            {
                memberDependencies = GetTypeDependencies(type);
                sceneConstructor.TypeToDependentMembers.Add(type, memberDependencies);
            }

            if (memberDependencies.Length > 0)
            {
                uint[] sceneIDs = new uint[memberDependencies.Length];

                for (int i = 0; i < memberDependencies.Length; i++)
                {
                    string memberName = memberDependencies[i].Name;

                    string value = Serializer.GetMemberValue(serializedData, memberName);
                    uint sceneID = uint.Parse(value);

                    sceneIDs[i] = sceneID;
                }

                sceneConstructor.Dependencies.Add(existingObject, sceneIDs);
            }
        }

        private static void ResolveSceneDependencies(SceneConstructor sceneConstructor)
        {
            foreach (KeyValuePair<object, uint[]> dependencies in sceneConstructor.Dependencies)
            {
                ResolveObjectDependencies(dependencies.Key, dependencies.Value, sceneConstructor);
            }
        }

        private static void ResolveObjectDependencies(object existingObject, uint[] sceneIDs, SceneConstructor sceneConstructor)
		{
			Type componentType = existingObject.GetType();

            MemberInfo[] memberInfos = sceneConstructor.TypeToDependentMembers[componentType];

            // inject directly with Injectables. No need to check for Infos here, this is done earlies.

            for (int i = 0; i < memberInfos.Length; i++)
            {
                uint sceneID = sceneIDs[i];
                EntityID entityID = sceneConstructor.SceneIDToEntityID[sceneIDs[i]];
                if (entityID == null)
                {
                    Debug.WriteLine($"EntityID of sceneID {sceneID} not found");
                    continue;
                }

                Type memberType = memberInfos[i] is FieldInfo fieldInfo ? fieldInfo.FieldType : ((PropertyInfo)memberInfos[i]).PropertyType;

                if (memberType == typeof(EntityID))
                {
                    ResolveDependency(memberInfos[i], existingObject, entityID);
                } else if (typeof(Component).IsAssignableFrom(memberType))
                {
                    Component componentToAssign = entityID.GetParent().GetComponentOfEntityID(entityID);
                    if (componentToAssign == null)
                    {
                        Debug.WriteLine($"Component not found");
                        continue;
                    }

                    ResolveDependency(memberInfos[i], existingObject, componentToAssign);
                }
            }
		}

		private static void ResolveDependency(MemberInfo memberInfo, object target, object value)
		{
			if (memberInfo is FieldInfo fieldInfo)
				fieldInfo.SetValue(target, value);
			else if (memberInfo is PropertyInfo propertyInfo)
				propertyInfo.SetValue(target, value);
		}

		private static MemberInfo[] GetTypeDependencies(Type type)
		{
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MemberInfo[] fieldInfos = type.GetFields(flags).Where(fieldInfo =>
            {
                SerializeAttribute serializable = fieldInfo.GetCustomAttribute<SerializeAttribute>();
                return serializable != null
                && (fieldInfo.FieldType == typeof(EntityID)
                || typeof(Component).IsAssignableFrom(fieldInfo.FieldType));
            }).ToArray();

            MemberInfo[] propertyInfos = type.GetProperties(flags).Where(property =>
            {
                return property.CanWrite && property.GetCustomAttribute<SerializeAttribute>() != null
                && (property.PropertyType == typeof(EntityID)
                || typeof(Component).IsAssignableFrom(property.PropertyType));
            }).ToArray();

            return (fieldInfos.Concat(propertyInfos)).ToArray();
        }

        private static void BuildSystems(SystemData[] systemDatas, SceneConstructor sceneConstructor)
        {
            foreach (var systemData in systemDatas)
            {
                ISystem system = BuildSystem(systemData);

                RegisterDependencies(system, systemData.serializedData, sceneConstructor);

                Systems.Add(system);
            }
        }

        private static ISystem BuildSystem(SystemData systemData)
        {
            Type systemType = Type.GetType(systemData.typeName);

            return Serializer.DeserializeSystem(systemData.serializedData, systemType);
        }

        public static SceneDefinition GetDefinition(RuntimeScene runtimeScene)
        {
            EntityData[] entityDatas = new EntityData[runtimeScene.Roots.Length];

            for (int i = 0; i < runtimeScene.Roots.Length; i++)
                entityDatas[i] = GetEntityData(runtimeScene.Roots[i]);

            EntityGraph entityGraph = new EntityGraph()
            {
                entities = entityDatas
            };

            SystemData[] systemDatas = GetSystemDatas(runtimeScene.GameSystems);

            SceneDefinition sceneDefinition = new SceneDefinition()
            {
                SystemDatas = systemDatas,
                EntityGraph = entityGraph
            };

            return sceneDefinition;
        }

        private static SystemData[] GetSystemDatas(ISystem[] gameSystems)
        {
            List<SystemData> systemDatas = new List<SystemData>();

            ISystem[] serializableSystems = GetSerializableSystems(gameSystems);

            foreach (var us in serializableSystems)
            {
                Type systemType = us.GetType();

                SystemData systemData = new SystemData()
                {
                    typeName = systemType.FullName,
                    serializedData = Serializer.SerializeSystem(us)
                };

                systemDatas.Add(systemData);
            }

            return systemDatas.ToArray();
        }

        private static ISystem[] GetSerializableSystems(ISystem[] gameSystems)
        {
            List<ISystem> serializableSystems = new();

            foreach (var system in gameSystems)
            {
                Type systemType = system.GetType();

                if (systemType.GetCustomAttribute<SerializeAttribute>() == null)
                    continue;

                serializableSystems.Add(system);
            }

            return serializableSystems.ToArray();
        }

        public static bool IsEntitySerializable(EntityID entity)
        {
            return entity.GetType().GetCustomAttribute<DoNotSerializeAttribute>() == null;
        }

        private static EntityData GetEntityData(EntityID entityID)
        {
            Component[] components = entityID.GetAll(); // Get all components

            List<ComponentData> componentDatas = new List<ComponentData>(components.Length);

            for (int i = 0; i < components.Length; i++) {
                Type componentType = components[i].GetType();
                if (componentType.GetCustomAttribute<DoNotSerializeAttribute>() != null)
                    continue;
                
                componentDatas.Add(GetComponentData(components[i], componentType));
            }

            EntityID[] children = entityID.GetChildren(false); // new EntityID[0]; // Get all children

            EntityData[] childrenDatas = new EntityData[children.Length];

            for (int i = 0; i < children.Length; i++)
                childrenDatas[i] = GetEntityData(children[i]);

            EntityData entityData = new EntityData()
            {
                id = (uint)entityID,
                active = entityID.active,
                components = componentDatas.ToArray(),
                children = childrenDatas
            };

            return entityData;
        }

        private static ComponentData GetComponentData(Component component, Type type)
        {
            ComponentData componentData = new ComponentData()
            {
                id = (uint)component.EntityID,
                active = component.EntityID.active,
                typeName = type.FullName,
                serializedData = Serializer.SerializeComponent(component)
            };

            return componentData;
        }
    }
}
