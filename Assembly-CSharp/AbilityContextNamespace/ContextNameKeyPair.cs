namespace AbilityContextNamespace
{
    public class ContextNameKeyPair
    {
        private string m_name;
        private int m_key;

        public ContextNameKeyPair(string name)
        {
            m_name = name;
            m_key = ContextVars.ToContextKey(name);
        }

        public int GetKey()
        {
            return m_key;
        }

        public string GetName()
        {
            return m_name;
        }
    }
}