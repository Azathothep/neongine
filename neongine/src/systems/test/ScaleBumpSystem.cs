using Microsoft.Xna.Framework;
using neongine;
using System.Collections;
using System.Collections.Generic;
using neon;
using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace neongine
{
    [Serialize, AllowMultiple]
    public class ScaleBumpSystem : IGameUpdateSystem
    {
        [Serialize]
        private float m_Speed;

        [Serialize]
        private Transform m_Transform;

        private Vector2 m_StartScale = Vector2.One;

        private float m_Timer = 0.0f;

        [JsonConstructor]
        private ScaleBumpSystem() { }

        public ScaleBumpSystem(Transform transform, float speed)
        {
            m_Speed = speed;
            m_Transform = transform;
            m_StartScale = Vector2.One; // To change back
        }

        public void Update(TimeSpan timeSpan)
        {
            m_Timer += (float)(timeSpan.TotalMilliseconds / 1000.0f) * m_Speed;
            if (m_Timer >= 1.0f || m_Timer <= -1.0f)
                m_Speed = -m_Speed;

            m_Transform.LocalScale = m_StartScale + Vector2.One * m_Timer;
        }
    }
}
