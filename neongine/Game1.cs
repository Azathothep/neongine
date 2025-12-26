using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using neon;
using System.IO;


namespace neongine
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //private SpriteFont m_SpriteFont;

        public static string RootDirectory;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            RootDirectory = Content.RootDirectory;

            // Debugger.Launch();
        }

        protected override void Initialize()
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.ApplyChanges();

            Neongine.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
			_spriteBatch = new SpriteBatch(GraphicsDevice);

            // m_SpriteFont = Content.Load<SpriteFont>("mainFont");

            Neongine.LoadCommonSystems(Window, _spriteBatch);

            Neongine.LoadCollisionSystems(_spriteBatch);

            Neongine.LoadEditorSystems(_spriteBatch);
            
            // TestCameraScene();

            TestCollisionResolutionScene();

            // TestBallScenes();

            // LoadScene();

            // InitializeContent();
        }

        private void TestBallScenes() {
            Rect bounds = new Rect(0.5f, 0.5f, 7, 4);

            Systems.Add(new VelocitySystem());
            Systems.Add(new AngleVelocitySystem());
            Systems.Add(new BallGenerationSystem(bounds));
            Systems.Add(new BouncingBallSystem(bounds));
        }

        private void TestCameraScene()
        {
            EntityID entityID = Neongine.Entity();
            Point point = entityID.Get<Point>();
            point.WorldPosition = new Vector3(1, 1, 0);
            entityID.Add(new Renderer(Assets.GetAsset<Texture2D>("ball")));
            
            EntityID cameraEntity = neon.Components.GetOwner(Camera.Main);
            Velocity cameraVelocity = cameraEntity.Add<Velocity>();

            Camera.Main.Zoom = 1.0f;
        
            Systems.Add(new ManualVelocityControlSystem(cameraVelocity, 0.03f));
            Systems.Add(new VelocitySystem());

            Systems.Add(new CameraZoomControllerSystem(2.0f));
        }

        private void TestCollisionResolutionScene() {

            EntityID entityID = Neongine.Entity();
            Point point = entityID.Get<Point>();
            point.WorldPosition = new Vector3(2, 0, 0);
            point.WorldRotation = 45.0f;
            Velocity entityVelocity = entityID.Add<Velocity>();
            entityID.Add(new Renderer(Assets.GetAsset<Texture2D>("ball")));
            //entityID.Add<IsDraggable>();
            entityID.Add(new Collider(new Geometry(GeometryType.Rectangle, 0.6f), 1.0f));

            EntityID wallID = Neongine.Entity();
            Point wallPoint = wallID.Get<Point>();
            Velocity wallVelocity = wallID.Add<Velocity>();
            wallPoint.WorldRotation = 37.0f;
            wallPoint.WorldPosition = new Vector3(-2, 0, 0);
            wallID.Add(new Renderer(Assets.GetAsset<Texture2D>("ball")));
            wallID.Add(new Collider(new Shape([
                                                new Vector2(0, 0.5f),
                                                new Vector2(0.6f, 0.3f),
                                                new Vector2(0.45f, -0.3f),
                                                new Vector2(0, -0.5f),
                                                new Vector2(-0.45f, 0),
                                                new Vector2(-0.3f, 0.3f)
                                            ]), 1.0f));

            // EntityID wallID2 = Neongine.Entity();
            // Point wallPoint2 = wallID2.Get<Point>();
            // wallPoint2.WorldPosition = new Vector3(4, 2, 0);
            // wallID2.Add(new Collider(new Geometry(GeometryType.Rectangle, 0.8f)));

            Systems.Add(new ManualVelocityControlSystem(entityVelocity, 0.03f));
            Systems.Add(new ManualVelocityControlSystem(wallVelocity, -0.02f));

            Systems.Add(new VelocitySystem());
            Systems.Add(new AngleVelocitySystem());

            neon.Components.GetOwner(Camera.Main).Get<Point>().WorldPosition = new Vector3(0, 0, 0);
        }

        private void LoadScene() {
            string scenePath = "./" + Content.RootDirectory + "/" + EditorSaveSystem.SavePath;
            string jsonString = File.ReadAllText(scenePath);

            Debug.WriteLine("Loading scene " + scenePath);

            SceneDefinition sceneDefinition = Serializer.DeserializeScene(jsonString);
            
            Scenes.Load(sceneDefinition);
        }

        private void InitializeContent()
        {
            Texture2D ballTexture = Assets.GetAsset<Texture2D>("ball");//Content.Load<Texture2D>("square");
            Texture2D squareTexture = Assets.GetAsset<Texture2D>("square");

            EntityID entityID_1 = Neongine.Entity();
            EntityID entityID_2 = Neongine.Entity();
            EntityID entityID_3 = Neongine.Entity();

            entityID_3.SetParent(entityID_1);

            var point1 = entityID_1.Get<Point>();
            var point2 = entityID_2.Get<Point>();
            var point3 = entityID_3.Get<Point>();

            point1.WorldPosition = new Vector3(1, 1, 0) * 200;
            point2.WorldPosition = Vector3.Right * 100 + Vector3.Up * 50;
            point3.LocalPosition = Vector3.Right * 75;

            entityID_1.Add(new Renderer(ballTexture));
            entityID_2.Add(new Renderer(squareTexture));
            entityID_3.Add(new Renderer(ballTexture));

            point1.WorldScale = Vector2.One * 2;
            point2.WorldScale = Vector2.One;
            point3.LocalScale = Vector2.One * 0.5f;

            entityID_1.Add<IsDraggable>();
            entityID_2.Add<IsDraggable>();

            entityID_1.Add(new AngleVelocity(0.5f));
            entityID_2.Add(new AngleVelocity(0.5f));
            entityID_3.Add(new AngleVelocity(-2.0f));

            entityID_1.Add(new Collider(new Geometry(GeometryType.Rectangle), 60, true));
            entityID_2.Add(new Collider(new Geometry(GeometryType.Rectangle), 60));
            entityID_3.Add(new Collider(new Geometry(GeometryType.Rectangle), 60));

            Systems.Add(new AngleVelocitySystem());

            CollisionSystem.OnTriggerEnter(entityID_2, (col) => Debug.WriteLine($"entity 2 entering trigger !"));
            CollisionSystem.OnTriggerExit(entityID_2, (col) => Debug.WriteLine($"entity 2 exiting trigger !"));
        }

        protected override void Update(GameTime gameTime)
        {
            // If unstarted entities : call IStartable

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            neon.Systems.Update(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            neon.Systems.Draw();

            base.Draw(gameTime);
        }
    }
}