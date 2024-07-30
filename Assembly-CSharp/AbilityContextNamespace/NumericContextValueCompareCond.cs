// ROGUES
// SERVER
using System;

namespace AbilityContextNamespace
{
    [Serializable]
    public class NumericContextValueCompareCond
    {
        public string m_contextName;
        public bool m_nonActorSpecificContext; // removed in rogues
        public ContextCompareOp m_compareOp;
        public float m_testValue;
        public bool m_ignoreIfNoContext; // removed in rogues
        private int m_contextKey;

        public int GetContextKey()
        {
            if (m_contextKey == 0)
            {
                m_contextKey = ContextVars.ToContextKey(m_contextName);
            }

            return m_contextKey;
        }

        public NumericContextValueCompareCond GetCopy()
        {
            return MemberwiseClone() as NumericContextValueCompareCond;
        }
    }
}