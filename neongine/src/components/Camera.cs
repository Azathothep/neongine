using neon;
using Microsoft.Xna.Framework;

namespace neongine
{
    /// <summary>
    /// Controls the render view
    /// </summary>
    public class Camera : Component, IAwakable
    {
        /// <summary>
        /// The camera currently being rendered
        /// </summary>
        public static Camera Main;

        public Transform Transform => m_Transform;
        private Transform m_Transform;
        
        /// <summary>
        /// The camera resolution, in pixels per units
        /// </summary>
        [Serialize] public int Resolution = 100;

        /// <summary>
        /// The camera zoom level
        /// </summary>
        [Serialize] public float Zoom = 1.0f;

        /// <summary>
        /// The screen dimensions, in pixels
        /// </summary>
        [Serialize] public Vector2 ScreenDimensions { get; private set; }

        /// <summary>
        /// The camera dimensions, in world space units
        /// </summary>
        public Vector2 WorldDimensions { get => ScreenDimensions / Resolution / Zoom; }

        /// <summary>
        /// The screen pixels offset, equals to ScreenDimensions / 2
        /// </summary>
        [Serialize] public Vector2 ScreenOffset { get; private set; }

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
            m_Transform = Components.GetOwner(this).Get<Transform>();
        }

        public override Component Clone() => new Camera(this);

        /// <summary>
        /// Converts a screen-space coordinate to world-space
        /// </summary>
        public Vector2 ScreenToWorld(Vector2 v)
        {
            Vector2 worldRelative = (v - ScreenOffset) / Resolution / Zoom;
            return new Vector2(worldRelative.X + m_Transform.WorldPosition.X, - (worldRelative.Y - m_Transform.WorldPosition.Y));
        }

        /// <summary>
        /// Converts a screen-space coordinate to world-space
        /// </summary>    
        public Vector2 ScreenToWorld(float x, float y)
        {
            x = (x - ScreenOffset.X) / Resolution / Zoom;
            y = (y - ScreenOffset.Y) / Resolution / Zoom;
            return new Vector2(x + m_Transform.WorldPosition.X, - (y - m_Transform.WorldPosition.Y));
        }

        /// <summary>
        /// Converts a world-space coordinate to screen-space
        /// </summary>
        public Vector2 WorldToScreen(Vector2 v) => ScreenOffset + new Vector2(v.X - m_Transform.WorldPosition.X, - (v.Y - m_Transform.WorldPosition.Y)) * Resolution * Zoom;
        
        /// <summary>
        /// Converts a world-space coordinate to screen-space
        /// </summary>
        public Vector2 WorldToScreen(float x, float y) => ScreenOffset + new Vector2((x - m_Transform.WorldPosition.X) * Resolution * Zoom, - (y - m_Transform.WorldPosition.Y) * Resolution * Zoom);
        
        /// <summary>
        /// Converts a world-space value (ex. a radius) to screen-space. Do not use for coordinates.
        /// </summary>
        public float WorldToScreen(float f) => f * Resolution * Zoom;
    }
}