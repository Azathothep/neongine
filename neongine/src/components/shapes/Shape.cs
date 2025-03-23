namespace neongine
{
    public struct Shape
    {
        public enum Type {
            Circle,
            Rectangle
        }

        [Serialize]
        public Type ShapeType;

        [Serialize]
        public float Width;

        [Serialize]
        public float Height;

        public Shape(Type shapeType, float width, float height = 1) {
            ShapeType = shapeType;
            Width = width;
            Height = height;
        }
    }
}
