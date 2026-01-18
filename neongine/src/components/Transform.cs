using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using neon;

namespace neongine
{
    /// <summary>
    /// Stores the coordinates, rotation and scale of the entity. Each entity created using <c>Neongine.Entity()</c> has one by default.
    /// </summary>
    public class Transform : Component, IAwakable
    {
        /// <summary>
        /// Flag this Transform's position and all its children Transform's positions as dirty
        /// </summary>
        private void SetPositionDirty()
        {
            m_WorldPosition.IsDirty = true;
            foreach (var c in m_Children)
                c.SetPositionDirty();
        }
  
        /// <summary>
        /// Flag this Transform's rotation and all its children Transform's rotations as dirty
        /// </summary>
        private void SetRotationDirty()
        {
            m_WorldRotation.IsDirty = true;

            foreach (var c in m_Children)
                c.SetRotationDirty();

            SetPositionDirty();
        }
  
        /// <summary>
        /// Flag this Transform's scale and all its children Transform's scale as dirty
        /// </summary>
        private void SetScaleDirty()
        {
            m_WorldScale.IsDirty = true;

            foreach (var c in m_Children)
                c.SetScaleDirty();

            SetPositionDirty();
        }

        /// <summary>
        /// The entity's children's Transforms
        /// </summary>
        private Transform[] m_Children;

        /// <summary>
        /// The entity's parent's Transform.
        /// </summary>
        private Transform m_Parent;

        /// <summary>
        /// The entity's parent's world position. If the entity is root, returns Vector3.Zero. 
        /// </summary>
        private Vector3 m_ParentWorldPosition
        {
            get => m_Parent != null ? m_Parent.WorldPosition : Vector3.Zero;
        }

        /// <summary>
        /// The entity's parent's world rotation. If the entity is root, returns 0.0f. 
        /// </summary>
        private float m_ParentWorldRotation
        {
            get => m_Parent != null ? m_Parent.WorldRotation : 0.0f;
        }

        /// <summary>
        /// The entity's parent's world scale. If the entity is root, returns Vector2.One. 
        /// </summary>
        private Vector2 m_ParentWorldScale
        {
            get => m_Parent != null ? m_Parent.WorldScale : Vector2.One;
        }

        private CachedValue<Vector3> m_WorldPosition;
        private CachedValue<float> m_WorldRotation;
        private CachedValue<Vector2> m_WorldScale;

        /// <summary>
        /// The entity's position, in world-space coordinates
        /// </summary>
        public Vector3 WorldPosition
        {
            get => m_WorldPosition;
            set => LocalPosition = (value.Rotate(-m_ParentWorldRotation) - m_ParentWorldPosition) / m_ParentWorldScale.ToVector3();
        }

        /// <summary>
        /// The X and Y values of the entity's position, in world-space coordinates
        /// </summary>
        public Vector2 WorldPosition2D {
            get => new Vector2(WorldPosition.X, WorldPosition.Y);
        }

        /// <summary>
        /// The entity's rotation, in degrees, in world-space coordinates
        /// </summary>
        public float WorldRotation
        {
            get => m_WorldRotation;
            set => LocalRotation = value - m_ParentWorldRotation;
        }

        /// <summary>
        /// The entity's scale, in world-space coordinates
        /// </summary>
        public Vector2 WorldScale
        {
            get => m_WorldScale;
            set => LocalScale = value / m_ParentWorldScale;
        }

        /// <summary>
        /// Update the world position using its parent's WorldPosition and its own LocalPosition
        /// </summary>
        private Vector3 UpdateWorldPosition()
        {
            return (LocalPosition.Rotate(m_ParentWorldRotation) * m_ParentWorldScale.ToVector3()) + m_ParentWorldPosition;
        }

        /// <summary>
        /// Update the world rotation using its parent's WorldRotation and its own LocalRotation
        /// </summary>
        private float UpdateWorldRotation()
        {
            return (m_ParentWorldRotation + LocalRotation) % 360;
        }

        /// <summary>
        /// Update the world scale using its parent's WorldScale and its own LocalScale
        /// </summary>
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

        /// <summary>
        /// The entity's position, relative to it's parent's position, rotation and scale
        /// </summary>
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
        
        /// <summary>
        /// The entity's rotation, in degrees, relative to its parent's rotation
        /// </summary>
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
        
        /// <summary>
        /// The entity's scale, relative to its parent's scale
        /// </summary>
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

        /// <summary>
        /// The Upward vector, relative to the entity's rotation
        /// </summary>
        public Vector2 Up
        {
            get => Vector2.UnitY.Rotate(WorldRotation);
        } 

        /// <summary>
        /// The Right vector, relative to the entity's rotation
        /// </summary>
        public Vector2 Right
        {
            get => Vector2.UnitX.Rotate(WorldRotation);
        } 

        public Transform(Vector3 position, float rotation, Vector2 scale)
        {
            this.m_LocalPosition = position;
            this.m_LocalRotation = rotation;
            this.m_LocalScale = scale;

            m_WorldPosition = new CachedValue<Vector3>(UpdateWorldPosition, position);
            m_WorldRotation = new CachedValue<float>(UpdateWorldRotation, rotation);
            m_WorldScale = new CachedValue<Vector2>(UpdateWorldScale, scale);
        }

        public Transform(Vector3 position) : this(position, 0.0f, Vector2.One) { }

        public Transform(Vector3 position, float rotation) : this(position, rotation, Vector2.One) { }

        public Transform() : this(Vector3.Zero, 0.0f, Vector2.One) {}

        public Transform(Transform other) : this(other.WorldPosition, other.WorldRotation, other.WorldScale) { }

        public void Awake()
        {
            EntityID ownerID = Components.GetOwner(this);

            UpdateParent();
            UpdateChildren();

            Hooks.Add(EntityHook.OnNewParent, UpdateParent, ownerID);
            Hooks.Add(EntityHook.OnNewChild, UpdateChildren, ownerID);
        }

        /// <summary>
        /// Called when this entity's parent has changed
        /// </summary>
        private void UpdateParent()
        {
            EntityID ownerID = Components.GetOwner(this);

            if (ownerID.depth == 0)
                return;

            m_Parent = ownerID.GetParent().Get<Transform>();
        }

        /// <summary>
        /// Called when one of this entity's children has changed (added or removed)
        /// </summary>
        private void UpdateChildren()
        {
            EntityID owner = Components.GetOwner(this);
            if (owner == null)
            {
                m_Children = new Transform[0];
                return;
            }

            m_Children = owner.GetInChildren<Transform>();
        }

        public override Component Clone()
        {
            return new Transform(this);
        }
    }
}
