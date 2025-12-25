using neon;

namespace neongine
{
    public class Camera : IComponent
    {
        public static Camera Main;

        public EntityID EntityID { get; private set; }
        
        public float Factor = 1.0f;

        public Camera()
        {
            
        }

        public Camera(Camera other) { }
        public IComponent Clone() => new Camera(this);
    }
}