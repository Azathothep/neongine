using neon;
using Newtonsoft.Json;

namespace neongine
{
    public class Name : IComponent
    {
        public EntityID EntityID { get; private set; }

        [Serialize]
        private string m_Value;

        public string Value => m_Value;

        private Name() { }

        public Name(string value)
        {
            m_Value = value;
        }

        public Name(Name other) : this(other.Value) { }

        public IComponent Clone()
        {
            return new Name(this);
        }
    }
}
