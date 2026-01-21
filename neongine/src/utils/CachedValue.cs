using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neongine
{
    /// <summary>
    /// Wrapper to store, cache and update values
    /// </summary>
    public class CachedValue<T>
    {
        private bool m_IsDirty = true;

        /// <summary>
        /// True if the value need to be updated
        /// </summary>
        public bool IsDirty
        {
            get => m_IsDirty;
            set => m_IsDirty = value;
        }

        /// <summary>
        /// The function required to update the value
        /// </summary>
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
