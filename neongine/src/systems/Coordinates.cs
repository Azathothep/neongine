using Microsoft.Xna.Framework;

namespace neongine
{
    public static class Coordinates
    {
        public static int Resolution = 100;
        
        public static Vector2 FromPixels(Vector2 v) => (v / Resolution);
        public static Vector2 FromPixels(float x, float y) => new Vector2(x / Resolution, y / Resolution);
        public static float FromPixels(float f) => f / Resolution;

        public static Vector2 ToPixels(Vector2 v) => (v * Resolution);
        public static Vector2 ToPixels(float x, float y) => new Vector2(x * Resolution, y * Resolution);
        public static float ToPixels(float f) => f * Resolution;
    }
}

