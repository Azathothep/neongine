using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using neon;
using neongine.editor;

namespace neongine
{
    /// <summary>
    /// Base functionalities to start Neongine
    /// </summary>
    public static class Neongine
    {
        /// <summary>
        /// Initializes the base underlying systems
        /// </summary>
        /// <param name="contentManager"></param>
        public static void Initialize(ContentManager contentManager)
        {
            Neon.Architecture architecture = Neon.Initialize();

            neongine.Systems.Initialize();

            Assets.SetManager(contentManager);

            Serializer.SetSerializer(new NewtonsoftJsonSerializer());
        }

        /// <summary>
        /// Load the most common game systems
        /// </summary>
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

        /// <summary>
        /// Load the collision-related systems
        /// </summary>
        public static void LoadCollisionSystems(SpriteBatch spriteBatch) {
            QuadtreeSpacePartitioner qsp = new QuadtreeSpacePartitioner(spriteBatch);
            
            Systems.Add(qsp);

            Systems.Add(new CollisionSystem(qsp,
                                            new SATCollisionDetector(),
                                            new VelocityCollisionResolver()));

            Systems.Add(new EditorColliderVisualizer());
        }

        /// <summary>
        /// Load the editor systems
        /// </summary>
        public static void LoadEditorSystems()
        {
            Systems.Add(new EditorDragSystem(0.05f));
            Systems.Add(new EditorSaveSystem());
            Systems.Add(new EditorGridSystem());
            Systems.Add(new EditorCameraControllerSystem(2.0f, 2.0f));
            Systems.Add(new EditorPlayModeSystem());
        }

        /// <summary>
        /// Create a new entity with a <c>Name</c> and a <c>Transform</c> component, initialized to the provided values
        /// </summary>
        public static EntityID Entity(string name = "new_entity", Vector3 position = default, float rotation = 0.0f, Vector2 scale = default)
        {
            EntityID entityID = Entities.GetID();

            entityID.Add(new Transform(position, rotation, scale == default ? Vector2.One : scale));
            entityID.Add(new Name(name));

            return entityID;
        }
    }
}
