using Newtonsoft.Json;
using System.Diagnostics;
using neon;
using System;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace neongine
{
    /// <summary>
    /// Implements <c>ISerializer</c> using Newtonsoft.Json
    /// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
        public static string GUID = "id";

        private JsonSerializerSettings ComponentSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = SerializeContractResolver.Instance,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = {
                new ComponentConverter()
            },
        };

        public JsonSerializerSettings SystemSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = SerializeContractResolver.Instance,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = {
                new SystemConverter()
            },
        };

        private JsonSerializerSettings m_SceneSerializerSettings = new JsonSerializerSettings()
        {
            Converters = {
                new SceneDefinitionConverter()
            }
        };

        public string SerializeComponent<T>(T component) where T : Component
        {
            string jsonString = JsonConvert.SerializeObject(component, ComponentSerializerSettings);

            // Debug.WriteLine($"converted {component.GetType()} component: {jsonString}");

            return jsonString;
        }

        public Component DeserializeComponent(string serializedData, Type type)
        {
            return (Component)JsonConvert.DeserializeObject(serializedData, type, ComponentSerializerSettings);
        }

        public string SerializeSystem<T>(T system) where T : ISystem
        {
            return JsonConvert.SerializeObject(system, system.GetType(), SystemSerializerSettings);
        }

        public ISystem DeserializeSystem(string serializedData, Type type)
        {
            return (ISystem)JsonConvert.DeserializeObject(serializedData, type, SystemSerializerSettings);
        }

        public string SerializeScene(SceneDefinition sceneDefinition)
        {
            return JsonConvert.SerializeObject(sceneDefinition, m_SceneSerializerSettings);
        }

        public SceneDefinition DeserializeScene(string serializedData)
        {
            return JsonConvert.DeserializeObject<SceneDefinition>(serializedData, m_SceneSerializerSettings);
        }

        /// <summary>
        /// Get the value of the taget member in the provided serialized object
        /// </summary>
        public string GetMemberValue(string serializedData, string memberName)
        {
            JObject obj = JObject.Parse(serializedData);

            JToken token = obj[memberName];
            if (token == null)
            {
                Debug.WriteLine($"Token for {memberName} not found in {serializedData}");
                return null;
            }

            JToken idToken = token[NewtonsoftJsonSerializer.GUID];
            if (token == null)
            {
                Debug.WriteLine($"idToken for {memberName} not found in {serializedData}");
                return null;
            }

            return (string)idToken;
        }
    }
}
