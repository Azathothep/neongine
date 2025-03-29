using Newtonsoft.Json;
using System.Diagnostics;
using neon;
using System;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace neongine
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private JsonSerializerSettings ComponentSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = SerializeContractResolver.Instance,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = {
                new ComponentConverter(),
            },
        };

        public JsonSerializerSettings SystemSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = SerializeContractResolver.Instance,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = AssetConverters.Converters.Concat(new JsonConverter[] {
                        new ComponentIDConverter(),
                        new EntityIDConverter(),
            }).ToArray()
        };

        private JsonSerializerSettings m_SceneSerializerSettings = new JsonSerializerSettings()
        {
            Converters = {
                new SceneDefinitionConverter()
            }
        };

        public string SerializeComponent<T>(T component) where T : class, IComponent
        {
            string jsonString = JsonConvert.SerializeObject(component, ComponentSerializerSettings);

            // Debug.WriteLine($"converted {component.GetType()} component: {jsonString}");

            return jsonString;
        }

        public IComponent DeserializeComponent(string serializedData, Type type)
        {
            return (IComponent)JsonConvert.DeserializeObject(serializedData, type, ComponentSerializerSettings);
        }

        public string SerializeSystem<T>(T system) where T : IGameSystem
        {
            return JsonConvert.SerializeObject(system, SystemSerializerSettings);
        }

        public IGameSystem DeserializeSystem(string serializedData, Type type)
        {
            return (IGameSystem)JsonConvert.DeserializeObject(serializedData, type, SystemSerializerSettings);
        }

        public string SerializeScene(SceneDefinition sceneDefinition)
        {
            return JsonConvert.SerializeObject(sceneDefinition, m_SceneSerializerSettings);
        }

        public SceneDefinition DeserializeScene(string serializedData)
        {
            return JsonConvert.DeserializeObject<SceneDefinition>(serializedData, m_SceneSerializerSettings);
        }

        public string GetMemberValue(string serializedData, string memberName)
        {
            JObject obj = JObject.Parse(serializedData);

            JToken token = obj[memberName];
            if (token == null)
            {
                Debug.WriteLine($"Token for {memberName} not found in {serializedData}");
                return null;
            }

            JToken idToken = token[EntityIDConverter.IDKey];
            if (token == null)
            {
                Debug.WriteLine($"idToken for {memberName} not found in {serializedData}");
                return null;
            }

            return (string)idToken;
        }
    }
}
