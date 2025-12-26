using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using neon;

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

        public static void LoadCommonSystems(GameWindow gameWindow, SpriteBatch spriteBatch)
        {
            EntityID cameraEntity = Neongine.Entity();

            Vector2 screenDimensions = new Vector2(gameWindow.ClientBounds.Width, gameWindow.ClientBounds.Height);

            cameraEntity.Add(new Camera(screenDimensions));

            Systems.Add(new RenderingSystem(spriteBatch));
        }

        public static void LoadCollisionSystems(SpriteBatch spriteBatch) {
            QuadtreeSpacePartitioner qsp = new QuadtreeSpacePartitioner(spriteBatch);
            
            Systems.Add(qsp);

            Systems.Add(new CollisionSystem(qsp,
                                            new SATCollisionDetector(),
                                            new VelocityCollisionResolver()));
        }

        public static void LoadEditorSystems(SpriteBatch spriteBatch)
        {
            Systems.Add(new DragSystem(spriteBatch, 8.0f));
            Systems.Add(new EditorSaveSystem());
            Systems.Add(new GridSystem());
        }

        public static EntityID Entity(string name = null)
        {
            EntityID entityID = Entities.GetID();

            name = name == null ? "new_entity" : name;

            entityID.Add<Point>();
            entityID.Add(new Name(name));

            return entityID;
        }

        public static void Start() {
            EntityID[] entities = Entities.GetRoots();
        }
    }
}
