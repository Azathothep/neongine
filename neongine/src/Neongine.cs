using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using neon;
using System.Collections.Generic;

namespace neongine
{
    public static class Neongine
    {
        public static void Initialize(ContentManager contentManager)
        {
            Neon.Architecture architecture = Neon.Initialize();

            Assets.SetManager(contentManager);

            Serializer.SetSerializer(new NewtonsoftJsonSerializer());
        }

        public static void LoadCommonSystems(SpriteBatch spriteBatch)
        {
            Systems.Add(new RenderingSystem(spriteBatch));

            Systems.Add(new CollisionSystem(new NoSpacePartitioner(),
                                            new SeparatingAxisCollisionDetector(),
                                            new NoCollisionResolver()));
        }

        public static void LoadEditorSystems(SpriteBatch spriteBatch)
        {
            Systems.Add(new DragSystem(spriteBatch, 8.0f));
            Systems.Add(new EditorSaveSystem());
        }

        public static EntityID Entity(string name = null)
        {
            EntityID entityID = Entities.GetID();

            name = name == null ? "new_entity" : name;

            entityID.Add<Point>();
            entityID.Add(new Name(name));

            return entityID;
        }
    }
}
