using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using neongine.editor;
using neon;

namespace neongine
{
    public class ExampleGame : neongine.IGame
    {
        public int WindowWidth => 800;
        public int WindowHeight => 480;

        public void EditorLoad()
        {
            
        }
        
        public void GameLoad()
        {
            EntityID entity = Neongine.Entity("first_entity", Vector3.One, 4.5f, Vector2.One * 1.3f);

            entity.Add<Velocity>()
                    .Add(new AngleVelocity(2.0f))
                    .Add(new Collider(new Geometry(GeometryType.Circle), isTrigger: true));
        }
    }
}

