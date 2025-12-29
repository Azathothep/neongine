using System;
using System.Data;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using neon;

namespace neongine {
    [DoNotSerialize]
    public class Bounds
    {
        [Serialize]
        public float X;

        [Serialize]
        public float Y;

        [Serialize]
        public float Width;

        [Serialize]
        public float Height;

        public Bounds() {

        }

        public Bounds(Bounds other) {
            this.X = other.X;
            this.Y = other.Y;
            this.Width = other.Width;
            this.Height = other.Height;
        }

        public Bounds(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Bounds(Shape shape) {
            Update(shape);
        }

        public void Update(Shape shape) {
            if (shape.Vertices == null) {
                Debug.WriteLine("Cannot create bounds : shape has no vertices !");
                return;
            } else if (shape.IsPolygon)
                BuildPolygonBounds(shape.Vertices);
            else
                BuildCircleBounds(shape.Radius);
        }

        private void BuildCircleBounds(float radius) {
            this.X = -radius;
            this.Y = -radius;
            this.Width = radius * 2;
            this.Height = radius * 2;
        }

        private void BuildPolygonBounds(Vector2[] vertices) {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;

            foreach (var vertice in vertices) {
                if (vertice.X < xMin)
                    xMin = vertice.X;
                if (vertice.X > xMax)
                    xMax = vertice.X;
                if (vertice.Y < yMin)
                    yMin = vertice.Y;
                if (vertice.Y > yMax)
                    yMax = vertice.Y;                    
            }

            this.X = xMin;
            this.Y = yMax;
            this.Width = xMax - xMin;
            this.Height = yMax - yMin;
        }

        public static bool Crossing(Vector2 p1, Bounds b1, Vector2 p2, Bounds b2) {
            (Vector2 lp, Bounds lb, Vector2 rp, Bounds rb) = p1.X + b1.X < p2.X + b2.X ? (p1, b1, p2, b2) : (p2, b2, p1, b1);
            (Vector2 tp, Bounds tb, Vector2 bp, Bounds bb) = p1.Y + b1.Y > p2.Y + b2.Y ? (p1, b1, p2, b2) : (p2, b2, p1, b1);

            return ((rp.X + rb.X) <= (lp.X + lb.Width / 2))
            && ((bp.Y + bb.Y) >= (tp.Y - tb.Height / 2));
        }
    }
}

