using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    public class CachedValue<T>
    {
        private bool m_IsDirty = true;
        public bool IsDirty
        {
            get => m_IsDirty;
            set => m_IsDirty = value;
        }

        private Func<T> m_UpdateValue;

        private T m_CachedValue;

        public CachedValue(Func<T> updateValue, T initValue = default)
        {
            m_UpdateValue = updateValue;
            m_CachedValue = initValue;
        }

        public T Get()
        {
            if (m_IsDirty)
            {
                m_CachedValue = m_UpdateValue();
                m_IsDirty = false;
            }

            return m_CachedValue;
        }

        public static implicit operator T(CachedValue<T> cachedValue) => cachedValue.Get();
    }
}
