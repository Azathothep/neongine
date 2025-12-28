using neon;
using Microsoft.Xna.Framework;

namespace neongine
{
    public class Camera : Component, IAwakable
    {
        public static Camera Main;

        public Point Point => m_Point;
        private Point m_Point;
        
        [Serialize] public int Resolution = 100;
        [Serialize] public float Zoom = 1.0f;

        [Serialize] public Vector2 ScreenDimensions { get; private set; }
        [Serialize] public Vector2 ScreenOffset { get; private set; }
        public Vector2 WorldDimensions { get => ScreenDimensions / Resolution / Zoom; }

        public Camera()
        {
            Camera.Main = this;
        }

        public Camera(Vector2 screenDimensions) : this()
        {
            ScreenDimensions = screenDimensions;
            ScreenOffset = screenDimensions / 2;
        }

        public Camera(Camera other) : this()
        {
            this.ScreenDimensions = other.ScreenDimensions;
            this.ScreenOffset = other.ScreenOffset;
            this.Resolution = other.Resolution;
            this.Zoom = other.Zoom;
        }

        public void Awake()
        {
            m_Point = Components.GetOwner(this).Get<Point>();
        }

        public override Component Clone() => new Camera(this);

        public Vector2 WorldToScreen(Vector2 v) => ScreenOffset + new Vector2(v.X - m_Point.WorldPosition.X, - (v.Y - m_Point.WorldPosition.Y)) * Resolution * Zoom;
        public Vector2 WorldToScreen(float x, float y) => ScreenOffset + new Vector2((x - m_Point.WorldPosition.X) * Resolution * Zoom, - (y - m_Point.WorldPosition.Y) * Resolution * Zoom);
        public float WorldToScreen(float f) => f * Resolution * Zoom;
    }
}