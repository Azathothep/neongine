using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace neongine
{
    public enum GeometryType {
        Circle,
        Rectangle
    }

    /// <summary>
    /// A basic geometric shape with custom parameters. Can convert into a <c>Shape</c>.
    /// </summary>
    public struct Geometry
    {
        [Serialize]
        public GeometryType Type;

        [Serialize]
        public float Width;

        [Serialize]
        public float Height;

        public Geometry(GeometryType type) : this(type, 1, 1) {

        }

        public Geometry(GeometryType type, float width) : this(type, width, width) {

        }

        public Geometry(GeometryType type, float width, float height) {
            Type = type;
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj)
        {
            return obj is Geometry other && other.Type == this.Type && other.Width == this.Width && other.Height == this.Height;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static implicit operator Shape(Geometry geometry) {
            return new Shape(geometry);
        }
    }
}
