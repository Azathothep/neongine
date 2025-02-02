namespace neongine
{
    public struct Circle : IShape
    {
        public float radius;

        public Circle()
        {
            radius = 1;
        }

        public Circle(float radius)
        {
            this.radius = radius;
        }

        public IShape Clone() => new Circle(radius);
    }
}
