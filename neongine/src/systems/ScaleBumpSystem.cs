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
    [AllowMultiple]
    public class ScaleBumpSystem : IUpdateSystem
    {
        [Serialize]
        private float m_Speed;

        [Serialize]
        private Point m_Scale;

        private Vector2 m_StartScale = Vector2.One;

        private float m_Timer = 0.0f;

        [JsonConstructor]
        private ScaleBumpSystem() { }

        public ScaleBumpSystem(Point scale, float speed)
        {
            m_Speed = speed;
            m_Scale = scale;
            m_StartScale = Vector2.One; // To change back
        }

        public void Update(TimeSpan timeSpan)
        {
            m_Timer += (float)(timeSpan.TotalMilliseconds / 1000.0f) * m_Speed;
            if (m_Timer >= 1.0f || m_Timer <= -1.0f)
                m_Speed = -m_Speed;

            m_Scale.LocalScale = m_StartScale + Vector2.One * m_Timer;
        }
    }
}
