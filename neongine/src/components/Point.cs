using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using neon;

namespace neongine
{
    public class Point : Component, IAwakable
    {
        public bool DirtyPosition => m_WorldPosition.IsDirty;
        public bool DirtyRotation => m_WorldRotation.IsDirty;
        public bool DirtyScale => m_WorldScale.IsDirty;

        private void SetPositionDirty()
        {
            m_WorldPosition.IsDirty = true;
            foreach (var c in m_Children)
                c.SetPositionDirty();
        }
        private void SetRotationDirty()
        {
            m_WorldRotation.IsDirty = true;

            foreach (var c in m_Children)
                c.SetRotationDirty();

            SetPositionDirty();
        }
        private void SetScaleDirty()
        {
            m_WorldScale.IsDirty = true;

            foreach (var c in m_Children)
                c.SetScaleDirty();

            SetPositionDirty();
        }

        private Point[] m_Children;
        private Point m_Parent;

        private Vector3 m_ParentWorldPosition
        {
            get => m_Parent != null ? m_Parent.WorldPosition : Vector3.Zero;
        } 
        private float m_ParentWorldRotation
        {
            get => m_Parent != null ? m_Parent.WorldRotation : 0.0f;
        }
        private Vector2 m_ParentWorldScale
        {
            get => m_Parent != null ? m_Parent.WorldScale : Vector2.One;
        }


        private CachedValue<Vector3> m_WorldPosition;
        private CachedValue<float> m_WorldRotation;
        private CachedValue<Vector2> m_WorldScale;

        public Vector3 WorldPosition
        {
            get => m_WorldPosition;
            set => LocalPosition = (value.Rotate(-m_ParentWorldRotation) - m_ParentWorldPosition) / m_ParentWorldScale.ToVector3();
        }
        public Vector2 WorldPosition2D {
            get => new Vector2(WorldPosition.X, WorldPosition.Y);
        }
        public float WorldRotation
        {
            get => m_WorldRotation;
            set => LocalRotation = value - m_ParentWorldRotation;
        }
        public Vector2 WorldScale
        {
            get => m_WorldScale;
            set => LocalScale = value / m_ParentWorldScale;
        }

        private Vector3 UpdateWorldPosition()
        {
            return (LocalPosition.Rotate(m_ParentWorldRotation) * m_ParentWorldScale.ToVector3()) + m_ParentWorldPosition;
        }
        private float UpdateWorldRotation()
        {
            return (m_ParentWorldRotation + LocalRotation) % 360;
        }
        private Vector2 UpdateWorldScale()
        {
            return m_ParentWorldScale * LocalScale;
        }

        [Serialize]
        private Vector3 m_LocalPosition = Vector3.Zero;

        [Serialize]
        private float m_LocalRotation = 0.0f;

        [Serialize]
        private Vector2 m_LocalScale = Vector2.One;

        public Vector3 LocalPosition
        {
            get => m_LocalPosition;
            set
            {
                if (m_LocalPosition == value)
                    return;

                m_LocalPosition = value;

                m_WorldPosition.IsDirty = true;

                foreach (var c in m_Children)
                    c.SetPositionDirty();
            }
        }
        public float LocalRotation
        {
            get => m_LocalRotation;
            set {
                if (m_LocalRotation == value)
                    return;

                m_LocalRotation = value % 360;

                m_WorldRotation.IsDirty = true;

                foreach (var c in m_Children)
                    c.SetRotationDirty();
            }
        }
        public Vector2 LocalScale
        {
            get => m_LocalScale;
            set
            {
                if (m_LocalScale == value)
                    return;

                m_LocalScale = value;

                m_WorldScale.IsDirty = true;

                foreach (var c in m_Children)
                    c.SetScaleDirty();
            }
        }

        public Vector2 Up
        {
            get => Vector2.UnitY.Rotate(WorldRotation);
        } 
        public Vector2 Right
        {
            get => Vector2.UnitX.Rotate(WorldRotation);
        } 

        public Point(Vector3 position, float rotation, Vector2 scale)
        {
            this.m_LocalPosition = position;
            this.m_LocalRotation = rotation;
            this.m_LocalScale = scale;

            m_WorldPosition = new CachedValue<Vector3>(UpdateWorldPosition, position);
            m_WorldRotation = new CachedValue<float>(UpdateWorldRotation, rotation);
            m_WorldScale = new CachedValue<Vector2>(UpdateWorldScale, scale);
        }

        public Point(Vector3 position) : this(position, 0.0f, Vector2.One) { }

        public Point(Vector3 position, float rotation) : this(position, rotation, Vector2.One) { }

        public Point() : this(Vector3.Zero, 0.0f, Vector2.One) {}

        public Point(Point other) : this(other.WorldPosition, other.WorldRotation, other.WorldScale) { }

        public void Awake()
        {
            EntityID ownerID = Components.GetOwner(this);

            UpdateParent();
            UpdateChildren();

            Hooks.Add(EntityHook.OnNewParent, UpdateParent, ownerID);
            Hooks.Add(EntityHook.OnNewChild, UpdateChildren, ownerID);
        }

        private void UpdateParent()
        {
            EntityID ownerID = Components.GetOwner(this);

            if (ownerID.depth == 0)
                return;

            m_Parent = ownerID.GetParent().Get<Point>();
        }

        private void UpdateChildren()
        {
            m_Children = Components.GetOwner(this).GetInChildren<Point>();
        }

        public override Component Clone()
        {
            return new Point(this);
        }
    }
}
