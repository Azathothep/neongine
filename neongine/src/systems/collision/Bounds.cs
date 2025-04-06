using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace neongine {
    [Serializable]
    public struct Bounds {
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

        public Bounds(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Bounds(Shape shape) {
            if (shape.Vertices == null) {
                Debug.WriteLine("Cannot create bounds : shape has no vertices !");
                return;
            } else if (shape.Vertices.Length == 2)
                BuildCircleBounds(shape.Vertices[1].X);
            else
                BuildPolygonBounds(shape.Vertices);
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
            this.Y = yMin;
            this.Width = xMax - xMin;
            this.Height = yMax - yMin;
        }

        public static bool Crossing(Vector3 p1, Bounds b1, Vector3 p2, Bounds b2) {
            (Vector3 lp, Bounds lb, Vector3 rp, Bounds rb) = p1.X + b1.X < p2.X + b2.X ? (p1, b1, p2, b2) : (p2, b2, p1, b1);
            (Vector3 tp, Bounds tb, Vector3 bp, Bounds bb) = p1.Y + b1.Y < p2.Y + b2.Y ? (p1, b1, p2, b2) : (p2, b2, p1, b1);

            return ((rp.X + rb.X) <= (lp.X + lb.Width / 2))
            && ((bp.Y + bb.Y) <= (tp.Y + tb.Height / 2));
        }
    }
}
