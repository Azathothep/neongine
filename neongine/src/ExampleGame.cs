using Microsoft.Xna.Framework;
using neon;

namespace neongine
{
    // Add this into your Program.cs to run the game using Neongine :
    //      using var application = new neongine.NeongineApplication(new ExampleGame());
    //      application.Run();

    public class ExampleGame : IGame
    {
        public int WindowWidth => 800;
        public int WindowHeight => 480;

        public void EditorLoad()
        {
            // Put here all your editor-related content. This won't be called in the published application.
        }
        
        public void GameLoad()
        {
            // Put here all your game-related content.

            // Example :

            EntityID entity = Neongine.Entity("first_entity", Vector3.One, 4.5f, Vector2.One * 1.3f);

            entity.Add<Velocity>()
                    .Add(new AngleVelocity(2.0f))
                    .Add(new Collider(new Geometry(GeometryType.Circle), isTrigger: true));
        }
    }
}

