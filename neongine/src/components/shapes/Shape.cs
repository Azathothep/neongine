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

        public Shape(Type shapeType, float width = 1, float height = 1) {
            ShapeType = shapeType;
            Width = width;
            Height = height;
        }
    }
}
