using System.Diagnostics;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using neon;
using neongine.editor;

namespace neongine
{
    public static class Systems
    {
        public class Storage {
            public SystemStorage<IUpdateSystem> Update = new();
            public SystemStorage<IDrawSystem> Draw = new();
        }

        public static Storage AllSystems;
        public static Storage GameSystems;
        public static Storage EditorSystems;
        public static Storage PlayModeSystems;

        public static void Initialize() {
            AllSystems = new();
            GameSystems = new();
            EditorSystems = new();
            PlayModeSystems = new();
        }

        public static void Add(ISystem system)
        {
            Type type = system.GetType();
            bool unique = type.GetCustomAttribute<AllowMultipleAttribute>() == null;

            if (unique && (AllSystems.Update.Systems.FirstOrDefault(s => s.GetType() == type) != null
                || AllSystems.Draw.Systems.FirstOrDefault(s => s.GetType() == type) != null))
            {
                Debug.WriteLine($"{type} has been added multiple times but is a unique system. Skipping.");
                return;
            }

            if (system is IUpdateSystem updateSystem)
            {
                AllSystems.Update.Add(updateSystem);
                
                if (updateSystem is IGameUpdateSystem gameUpdateSystem)
                    GameSystems.Update.Add(gameUpdateSystem);

                if (updateSystem is IEditorUpdateSystem editorUpdateSystem)
                    EditorSystems.Update.Add(editorUpdateSystem);

                if (updateSystem is IGameUpdateSystem
                || updateSystem is IEditorUpdateSystem editorSystem && editorSystem.ActiveInPlayMode)
                    PlayModeSystems.Update.Add(updateSystem);
            }

            if (system is IDrawSystem drawSystem)
            {
                AllSystems.Draw.Add(drawSystem);
                
                if (drawSystem is IGameDrawSystem gameDrawSystem)
                    GameSystems.Draw.Add(gameDrawSystem);

                if (drawSystem is IEditorDrawSystem editorDrawSystem)
                    EditorSystems.Draw.Add(editorDrawSystem);

                if (drawSystem is IGameDrawSystem
                || drawSystem is IEditorDrawSystem editorSystem && editorSystem.ActiveInPlayMode)
                    PlayModeSystems.Draw.Add(drawSystem);
            }
        }

        public static void Remove(ISystem system)
        {
            if (system is IUpdateSystem updateSystem)
            {
                AllSystems.Update.Remove(updateSystem);
                
                if (updateSystem is IGameUpdateSystem gameUpdateSystem)
                    GameSystems.Update.Remove(gameUpdateSystem);

                if (updateSystem is IEditorUpdateSystem editorUpdateSystem)
                    EditorSystems.Update.Remove(editorUpdateSystem);

                PlayModeSystems.Update.Remove(updateSystem);
            }

            if (system is IDrawSystem drawSystem)
            {
                AllSystems.Draw.Remove(drawSystem);
                
                if (drawSystem is IGameDrawSystem gameDrawSystem)
                    GameSystems.Draw.Remove(gameDrawSystem);

                if (drawSystem is IEditorDrawSystem editorDrawSystem)
                    EditorSystems.Draw.Remove(editorDrawSystem);

                PlayModeSystems.Draw.Remove(drawSystem);
            }
        }

        public static ISystem[] GetLoadedGameSystems()
        {
            List<ISystem> gameSystems = [.. GameSystems.Update.Systems];

            foreach (ISystem system in GameSystems.Draw.Systems)
            {
                if (gameSystems.Contains(system))
                    continue;

                gameSystems.Add(system);
            }

            return gameSystems.ToArray();
        }

        public static void Update(Storage storage, TimeSpan timeSpan)
        {
            for (int i = 0; i < storage.Update.Systems.Count; i++)
            {
                storage.Update.Systems[i].Update(timeSpan);
            }
        }

        public static void Draw(Storage storage)
        {
            for (int i = 0; i < storage.Draw.Systems.Count; i++)
            {
                storage.Draw.Systems[i].Draw();
            }
        }

        public static void Clear(Storage storage)
        {
            storage.Update.Systems.Clear();
            storage.Draw.Systems.Clear();
        }
    }
}
