using neon;
using Microsoft.Xna.Framework;

namespace neongine
{
    public class Camera : IComponent
    {
        public static Camera Main;

        public EntityID EntityID { get; private set; }
        
        public int Resolution = 100;
        public float Zoom = 1.0f;

        public Camera()
        {
            if (Camera.Main == null)
                Camera.Main = this;
        }

        public Camera(Camera other) { }
        public IComponent Clone() => new Camera(this);

        public Vector2 ScreenToWorld(Vector2 v) => v / Resolution;
        public Vector2 ScreenToWorld(float x, float y) => new Vector2(x / Resolution, y / Resolution);
        public float ScreenToWorld(float f) => f / Resolution;

        public Vector2 WorldToScreen(Vector2 v) => v * Resolution;
        public Vector2 WorldToScreen(float x, float y) => new Vector2(x * Resolution, y * Resolution);
        public float WorldToScreen(float f) => f * Resolution;
    }
}