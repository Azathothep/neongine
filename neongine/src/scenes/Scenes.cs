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
        private struct SceneSerializer
        {
            public HashSet<ISystem> ReferencedSystems = new();
            public Dictionary<Type, MemberInfo[]> TypeToDependentSystems = new();
            public SceneSerializer() { }
        }

		private struct SceneConstructor
		{
            public Dictionary<uint, object> SceneIDToObjectID = new();
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

            BuildSystems(scene.SystemDatas, scene.LoadedSystems, sceneConstructor);

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

            sceneConstructor.SceneIDToObjectID.Add(entityData.id, entityID);

            foreach (var componentData in entityData.components)
			{
                Component component = BuildComponent(entityID, componentData);
				sceneConstructor.SceneIDToObjectID.Add(componentData.id, component.EntityID);

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
                object o = sceneConstructor.SceneIDToObjectID[sceneIDs[i]];
                if (o == null)
                {
                    Debug.WriteLine($"EntityID of sceneID {sceneID} not found");
                    continue;
                }

                Type memberType = memberInfos[i] is FieldInfo fieldInfo ? fieldInfo.FieldType : ((PropertyInfo)memberInfos[i]).PropertyType;

                if (typeof(Component).IsAssignableFrom(memberType))
                {
                    EntityID entityID = (EntityID)o;
                    Component componentToAssign = entityID.GetParent().GetComponentOfEntityID(entityID);
                    if (componentToAssign == null)
                    {
                        Debug.WriteLine($"Component not found");
                        continue;
                    }

                    ResolveDependency(memberInfos[i], existingObject, componentToAssign);
                } else
                {
                    ResolveDependency(memberInfos[i], existingObject, o);
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
                || typeof(Component).IsAssignableFrom(fieldInfo.FieldType)
                || typeof(ISystem).IsAssignableFrom(fieldInfo.FieldType));
            }).ToArray();

            MemberInfo[] propertyInfos = type.GetProperties(flags).Where(property =>
            {
                return property.CanWrite && property.GetCustomAttribute<SerializeAttribute>() != null
                && (property.PropertyType == typeof(EntityID)
                || typeof(Component).IsAssignableFrom(property.PropertyType)
                || typeof(ISystem).IsAssignableFrom(property.PropertyType));
            }).ToArray();

            return (fieldInfos.Concat(propertyInfos)).ToArray();
        }

        private static void BuildSystems(SystemData[] systemDatas, uint[] loadedSystems, SceneConstructor sceneConstructor)
        {
            foreach (var systemData in systemDatas)
            {
                ISystem system = BuildSystem(systemData);

                sceneConstructor.SceneIDToObjectID.Add(systemData.id, system);

                RegisterDependencies(system, systemData.serializedData, sceneConstructor);

                if (loadedSystems.Contains(systemData.id))
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
            SceneSerializer sceneSerializer = new SceneSerializer();

            EntityData[] entityDatas = new EntityData[runtimeScene.Roots.Length];

            for (int i = 0; i < runtimeScene.Roots.Length; i++)
                entityDatas[i] = GetEntityData(runtimeScene.Roots[i], sceneSerializer);

            EntityGraph entityGraph = new EntityGraph()
            {
                entities = entityDatas
            };

            uint[] loadedSystems = GetLoadedSystemIDs(runtimeScene.GameSystems);

            foreach (var system in runtimeScene.GameSystems)
                sceneSerializer.ReferencedSystems.Add(system);

            SystemData[] systemDatas = GetSystemDatas(sceneSerializer);

            SceneDefinition sceneDefinition = new SceneDefinition()
            {
                SystemDatas = systemDatas,
                LoadedSystems = loadedSystems,
                EntityGraph = entityGraph
            };

            return sceneDefinition;
        }

        private static uint[] GetLoadedSystemIDs(ISystem[] systems)
        {
            ISystem[] serializableSystems = GetSerializableSystems(systems);

            uint[] systemIDs = new uint[serializableSystems.Length];

            for (int i = 0; i < serializableSystems.Length; i++)
                systemIDs[i] = (uint)serializableSystems[i].GetHashCode();

            return systemIDs;
        }

        private static SystemData[] GetSystemDatas(SceneSerializer sceneSerializer)
        {
            ISystem[] serializableSystems = GetSerializableSystems(sceneSerializer.ReferencedSystems);

            foreach (var system in serializableSystems)
                RegisterDependentSystems(system, sceneSerializer);

            serializableSystems = GetSerializableSystems(sceneSerializer.ReferencedSystems);

            List<SystemData> systemDatas = new List<SystemData>();

            foreach (var system in serializableSystems)
            {
                Type systemType = system.GetType();

                SystemData systemData = new SystemData()
                {
                    id = system == null ? 0 : (uint)system.GetHashCode(),
                    typeName = systemType.AssemblyQualifiedName,
                    serializedData = Serializer.SerializeSystem(system)
                };

                systemDatas.Add(systemData);
            }

            return systemDatas.ToArray();
        }

        private static ISystem[] GetSerializableSystems(IEnumerable<ISystem> gameSystems)
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

        private static EntityData GetEntityData(EntityID entityID, SceneSerializer sceneSerializer)
        {
            Component[] components = entityID.GetAll(); // Get all components

            List<ComponentData> componentDatas = new List<ComponentData>(components.Length);

            for (int i = 0; i < components.Length; i++) {
                Type componentType = components[i].GetType();
                if (componentType.GetCustomAttribute<DoNotSerializeAttribute>() != null)
                    continue;
                
                componentDatas.Add(GetComponentData(components[i], componentType));
                RegisterDependentSystems(components[i], sceneSerializer);
            }

            EntityID[] children = entityID.GetChildren(includeComponents: false);

            EntityData[] childrenDatas = new EntityData[children.Length];

            for (int i = 0; i < children.Length; i++)
                childrenDatas[i] = GetEntityData(children[i], sceneSerializer);

            EntityData entityData = new EntityData()
            {
                id = (uint)entityID,
                active = entityID.active,
                components = componentDatas.ToArray(),
                children = childrenDatas
            };

            return entityData;
        }

        private static void RegisterDependentSystems(object o, SceneSerializer sceneSerializer)
        {
            Type type = o.GetType();

            if (!sceneSerializer.TypeToDependentSystems.TryGetValue(type, out MemberInfo[] memberSystems))
            {
                memberSystems = GetDependentSystems(type);
                sceneSerializer.TypeToDependentSystems.Add(type, memberSystems);
            }

            if (memberSystems.Length == 0)
                return;
                
            for (int i = 0; i < memberSystems.Length; i++)
            {
                sceneSerializer.ReferencedSystems.Add(GetMemberSystem(o, memberSystems[i]));
            }
        }

        private static MemberInfo[] GetDependentSystems(Type type)
		{
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            MemberInfo[] fieldInfos = type.GetFields(flags).Where(fieldInfo =>
            {
                SerializeAttribute serializable = fieldInfo.GetCustomAttribute<SerializeAttribute>();
                return serializable != null
                && typeof(ISystem).IsAssignableFrom(fieldInfo.FieldType);
            }).ToArray();

            MemberInfo[] propertyInfos = type.GetProperties(flags).Where(property =>
            {
                return property.CanWrite && property.GetCustomAttribute<SerializeAttribute>() != null
                && typeof(ISystem).IsAssignableFrom(property.PropertyType);
            }).ToArray();

            return (fieldInfos.Concat(propertyInfos)).ToArray();
        }

        private static ISystem GetMemberSystem(object o, MemberInfo info)
        {
            if (info is FieldInfo field)
                return (ISystem)field.GetValue(o);
            else if (info is PropertyInfo property)
                return (ISystem)property.GetValue(o);

            return default;;
        }

        private static ComponentData GetComponentData(Component component, Type type)
        {
            ComponentData componentData = new ComponentData()
            {
                id = (uint)component.EntityID,
                active = component.EntityID.active,
                typeName = type.AssemblyQualifiedName,
                serializedData = Serializer.SerializeComponent(component)
            };

            return componentData;
        }
    }
}
