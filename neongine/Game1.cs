﻿using Microsoft.Xna.Framework;
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
            Neongine.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
			_spriteBatch = new SpriteBatch(GraphicsDevice);

            // m_SpriteFont = Content.Load<SpriteFont>("mainFont");

            Neongine.LoadCommonSystems(_spriteBatch);

            Neongine.LoadCollisionSystems(_spriteBatch);

            Neongine.LoadEditorSystems(_spriteBatch);
            
            TestCollisionResolutionScene();

            // TestBallScenes();

            // LoadScene();

            // InitializeContent();
        }

        private void TestBallScenes() {
            Rect bounds = new Rect(50, 50, 700, 400);

            Systems.Add(new VelocitySystem());
            Systems.Add(new AngleVelocitySystem());
            Systems.Add(new BallGenerationSystem(bounds));
            Systems.Add(new BouncingBallSystem(bounds));
        }

        private void TestCollisionResolutionScene() {

            EntityID entityID = Neongine.Entity();
            Point point = entityID.Get<Point>();
            point.WorldPosition = new Vector3(500, 200, 0);
            point.WorldRotation = 45.0f;
            Velocity entityVelocity = entityID.Add<Velocity>();
            entityID.Add(new Renderer(Assets.GetAsset<Texture2D>("ball")));
            //entityID.Add<IsDraggable>();
            entityID.Add(new Collider(new Geometry(GeometryType.Rectangle, 60), 1.0f));

            EntityID wallID = Neongine.Entity();
            Point wallPoint = wallID.Get<Point>();
            Velocity wallVelocity = wallID.Add<Velocity>();
            wallPoint.WorldRotation = 37.0f;
            wallPoint.WorldPosition = new Vector3(200, 200, 0);
            wallID.Add(new Renderer(Assets.GetAsset<Texture2D>("ball")));
            wallID.Add(new Collider(new Shape([
                                                new Vector2(0, 50),
                                                new Vector2(60, 30),
                                                new Vector2(45, -30),
                                                new Vector2(0, -50),
                                                new Vector2(-45,0),
                                                new Vector2(-30, 30)
                                            ]), 1.0f));

            // EntityID wallID2 = Neongine.Entity();
            // Point wallPoint2 = wallID2.Get<Point>();
            // wallPoint2.WorldPosition = new Vector3(400, 200, 0);
            // wallID2.Add(new Collider(new Geometry(GeometryType.Rectangle, 80)));

            Systems.Add(new ManualVelocityControlSystem(entityVelocity, 3.0f));
            Systems.Add(new ManualVelocityControlSystem(wallVelocity, -2.0f));

            Systems.Add(new VelocitySystem());
            Systems.Add(new AngleVelocitySystem());
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