using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using neon;
using Microsoft.Xna.Framework.Graphics;

namespace neongine.editor
{
    [Order(OrderType.After, typeof(RenderingSystem))]
    public class EditorPlayModeSystem : IEditorUpdateSystem, IEditorDrawSystem
    {
        public bool ActiveInPlayMode => true;

        public static bool IsPlayMode = false;

        public bool m_KeyWasDownOnPreviousFrame = false;

        private RuntimeScene m_RuntimeScene;
        private SceneDefinition m_SceneDefinition;

        public void Update(TimeSpan timeSpan)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            bool keyPressed = keyboardState.IsKeyDown(Keys.P);

            if (!m_KeyWasDownOnPreviousFrame && keyPressed)
                SwitchMode(IsPlayMode ? false : true);

            m_KeyWasDownOnPreviousFrame = keyPressed;

        }

        private void SwitchMode(bool playMode)
        {
            switch (playMode)
            {
                case true:
                    m_RuntimeScene = Scenes.GetRuntime();
                    m_SceneDefinition = Scenes.GetDefinition(m_RuntimeScene);
                    break;
                case false:
                    Scenes.Unload(m_RuntimeScene);
                    Scenes.Load(m_SceneDefinition);
                    break;
            }

            IsPlayMode = playMode;
        }

        public void Draw()
        {
            RenderingSystem.DrawText(IsPlayMode ? "Play" : "Editor", Vector2.Zero, 1, Color.White);
        }
    }
}

