using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using neon;
using System;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;


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

            // LoadScene();

            Neongine.LoadCommonSystems(_spriteBatch);

            Neongine.LoadEditorSystems(_spriteBatch);

            InitializeContent();
        }

        private void LoadScene() {
            string scenePath = "./" + Content.RootDirectory + "/" + EditorSaveSystem.SavePath;
            string jsonString = File.ReadAllText(scenePath);

            SceneDefinition sceneDefinition = Serializer.DeserializeScene(jsonString);
            
            Scenes.Load(sceneDefinition);
        }

        private void InitializeContent()
        {
            Texture2D ballTexture = Assets.GetAsset<Texture2D>("ball");//Content.Load<Texture2D>("square");
            Texture2D squareTexture = Assets.GetAsset<Texture2D>("square");

            EntityID entityID_1 = Neongine.Entity();
            EntityID entityID_2 = Neongine.Entity();

            var point1 = entityID_1.Get<Point>();
            var point2 = entityID_2.Get<Point>();

            point1.WorldPosition = new Vector3(1, 1, 0) * 200;
            point2.WorldPosition = Vector3.Right * 100 + Vector3.Up * 50;

            entityID_1.Add(new Renderer(ballTexture));
            entityID_2.Add(new Renderer(squareTexture));

            point1.WorldScale = Vector2.One * 2.0f;
            point2.WorldScale = Vector2.One;

            entityID_1.Add<Draggable>();
            entityID_2.Add<Draggable>();

            entityID_1.Add(new Collider(new Shape(Shape.Type.Circle), 35, true));
            entityID_2.Add(new Collider(new Shape(Shape.Type.Circle), 50));

            CollisionSystem.OnTriggerEnter(entityID_1, (col) => Debug.WriteLine($"entity 1 entering trigger !"));
            CollisionSystem.OnTriggerExit(entityID_1, (col) => Debug.WriteLine($"entity 1 exiting trigger !"));
        }

        protected override void Update(GameTime gameTime)
        {
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