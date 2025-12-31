using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using neon;
using neongine.editor;

namespace neongine
{
    public static class Neongine
    {
        public static void Initialize(ContentManager contentManager)
        {
            Neon.Architecture architecture = Neon.Initialize();

            neongine.Systems.Initialize();

            Assets.SetManager(contentManager);

            Serializer.SetSerializer(new NewtonsoftJsonSerializer());
        }

        public static void LoadCommonSystems(GameWindow gameWindow, SpriteBatch spriteBatch)
        {
            EntityID cameraEntity = Neongine.Entity();

            Vector2 screenDimensions = new Vector2(gameWindow.ClientBounds.Width, gameWindow.ClientBounds.Height);

            cameraEntity.Add(new Camera(screenDimensions));
            cameraEntity.Add<NotDraggable>();

            SpriteFont spriteFont = Assets.GetAsset<SpriteFont>("Arial");

            Systems.Add(new RenderingSystem(spriteBatch, spriteFont));

            Systems.Add(new VelocitySystem());
            Systems.Add(new AngleVelocitySystem());
        }

        public static void LoadCollisionSystems(SpriteBatch spriteBatch) {
            QuadtreeSpacePartitioner qsp = new QuadtreeSpacePartitioner(spriteBatch);
            
            Systems.Add(qsp);

            Systems.Add(new CollisionSystem(qsp,
                                            new SATCollisionDetector(),
                                            new VelocityCollisionResolver()));

            Systems.Add(new EditorColliderVisualizer());
        }

        public static void LoadEditorSystems()
        {
            Systems.Add(new EditorDragSystem(0.05f));
            Systems.Add(new EditorSaveSystem());
            Systems.Add(new EditorGridSystem());
            Systems.Add(new EditorCameraControllerSystem(2.0f, 2.0f));
            Systems.Add(new EditorPlayModeSystem());
        }

        public static EntityID Entity(string name = null)
        {
            EntityID entityID = Entities.GetID();

            name = name == null ? "new_entity" : name;

            entityID.Add<Transform>();
            entityID.Add(new Name(name));

            return entityID;
        }
    }
}
