using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using neongine.editor;

namespace neongine
{
    public class NeongineApplication : Game
    {
        public static string RootDirectory;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private IGame m_Game;

        public NeongineApplication(IGame game)
        {
            m_Game = game;

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;
            RootDirectory = Content.RootDirectory;
        }

        protected override void Initialize()
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = m_Game.WindowWidth;
            _graphics.PreferredBackBufferHeight = m_Game.WindowHeight;
            _graphics.ApplyChanges();

            Neongine.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
			_spriteBatch = new SpriteBatch(GraphicsDevice);

            Neongine.LoadCommonSystems(Window, _spriteBatch);

            Neongine.LoadCollisionSystems(_spriteBatch);

#if !NEONGINE_BUILD
            Neongine.LoadEditorSystems();

            m_Game.EditorLoad();
#endif            

            m_Game.GameLoad();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

#if !NEONGINE_BUILD
            if (EditorPlayModeSystem.IsPlayMode)
            {
                neongine.Systems.Update(neongine.Systems.PlayModeSystems, gameTime.ElapsedGameTime);
            } else
            {
                neongine.Systems.Update(neongine.Systems.EditorSystems, gameTime.ElapsedGameTime);
            }
#else
            neongine.Systems.Update(neongine.Systems.GameSystems, gameTime.ElapsedGameTime);
#endif
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
#if !NEONGINE_BUILD
            if (EditorPlayModeSystem.IsPlayMode)
            {
                GraphicsDevice.Clear(Color.Black);
                neongine.Systems.Draw(neongine.Systems.PlayModeSystems);
            } else
            {
                GraphicsDevice.Clear(Color.LightSlateGray);
                neongine.Systems.Draw(neongine.Systems.EditorSystems);
            }
#else
            GraphicsDevice.Clear(Color.Black);
            neongine.Systems.Draw(neongine.Systems.GameSystems);
#endif

            base.Draw(gameTime);
        }
    }
}