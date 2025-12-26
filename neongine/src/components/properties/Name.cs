using neon;
using Newtonsoft.Json;

namespace neongine
{
    public class Name : Component
    {
        [Serialize]
        private string m_Value;

        public string Value => m_Value;

        private Name() { }

        public Name(string value)
        {
            m_Value = value;
        }

        public Name(Name other) : this(other.Value) { }

        public override Component Clone()
        {
            return new Name(this);
        }
    }
}
