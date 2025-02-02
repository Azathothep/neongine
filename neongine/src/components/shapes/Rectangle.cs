namespace neongine
{
    public struct Rectangle : IShape
    {
        public float width;
        public float height;

        public Rectangle()
        {
            width = 1;
            height = 1;
        }

        public Rectangle(float length)
        {
            this.width = length;
            this.height = length;
        }

        public Rectangle(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public IShape Clone() => new Rectangle(width, height);
    }
}
