using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using neon;
using System;
using System.Diagnostics;
using System.IO;

namespace neongine.editor
{
    /// <summary>
    /// Add the ability to save a scene using Ctrl + S keys when in Editor
    /// </summary>
    public class EditorSaveSystem : IEditorUpdateSystem
    {
        public bool ActiveInPlayMode => true;
        
        public static string RelativeSavePath = "scenes/MainScene.json";
        public static string AbsoluteSavePath => "./" + NeongineApplication.RootDirectory + "/" + RelativeSavePath;

        private ButtonState m_PreviousState_S;
        private ButtonState m_PreviousState_L;

        public EditorSaveSystem()
        {
            
        }

        public void Update(TimeSpan timeSpan)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            bool savePressed =  keyboardState.IsKeyDown(Keys.LeftControl)
                                    && keyboardState.IsKeyDown(Keys.S)
                                    && m_PreviousState_S == ButtonState.Released;

            if (savePressed)
            {
                RuntimeScene runtimeScene = Scenes.GetRuntime();

                SceneDefinition sceneDefinition = Scenes.GetDefinition(runtimeScene);
                string jsonString = Serializer.SerializeScene(sceneDefinition);

                Debug.WriteLine("Saving to : " + EditorSaveSystem.AbsoluteSavePath);

                File.WriteAllText(EditorSaveSystem.AbsoluteSavePath, jsonString);

                Debug.WriteLine("Scene has been saved !");

                return;
            }

            bool loadPressed = keyboardState.IsKeyDown(Keys.LeftControl)
                        && keyboardState.IsKeyDown(Keys.L)
                        && m_PreviousState_L == ButtonState.Released;

            if (loadPressed)
            {
                Debug.WriteLine("Hot scene loading not yet implemented");
            }

            m_PreviousState_S = GetButtonState(keyboardState, Keys.S);
            m_PreviousState_L = GetButtonState(keyboardState, Keys.L);
        }

        private ButtonState GetButtonState(KeyboardState kbstate, Keys k) => kbstate.IsKeyDown(k) ? ButtonState.Pressed : ButtonState.Released;
    }
}
