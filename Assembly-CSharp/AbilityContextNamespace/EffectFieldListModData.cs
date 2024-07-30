// ROGUES
// SERVER
using System;
using System.Collections.Generic;

namespace AbilityContextNamespace
{
    [Serializable]
    public class EffectFieldListModData
    {
        public List<OnHitEffecField> m_prependEffectFields;
        public List<EffectFieldOverride> m_overrides;

        public List<OnHitEffecField> GetModdedEffectFieldList(List<OnHitEffecField> input)
        {
            List<OnHitEffecField> list = new List<OnHitEffecField>();
            foreach (OnHitEffecField effectField in m_prependEffectFields)
            {
                list.Add(effectField.GetCopy());
            }

            foreach (OnHitEffecField effectField in input)
            {
                OnHitEffecField overrideEntry = GetOverrideEntry(effectField.GetIdentifier());
                list.Add(
                    overrideEntry != null
                        ? overrideEntry.GetCopy()
                        : effectField.GetCopy());
            }

            return list;
        }

        private OnHitEffecField GetOverrideEntry(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }

            foreach (EffectFieldOverride effectFieldOverride in m_overrides)
            {
                string overrideIdentifier = effectFieldOverride.GetIdentifier();
                if (overrideIdentifier.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return effectFieldOverride.m_effectOverride;
                }
            }

            return null;
        }
    }
}