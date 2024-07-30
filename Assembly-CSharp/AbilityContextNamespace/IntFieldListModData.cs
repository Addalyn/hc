using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilityContextNamespace
{
    [Serializable]
    public class IntFieldListModData
    {
        [Header("-- Evaluated before original's int fields (first match would be used)")]
        public List<OnHitIntField> m_prependIntFields;
        [Header("-- Overrides to existing int fields")]
        public List<IntFieldOverride> m_overrides;

        public List<OnHitIntField> GetModdedIntFieldList(List<OnHitIntField> input)
        {
            List<OnHitIntField> list = new List<OnHitIntField>();
            foreach (OnHitIntField onHitIntField in m_prependIntFields)
            {
                list.Add(onHitIntField.GetCopy());
            }

            foreach (OnHitIntField onHitIntField in input)
            {
                SingleOnHitIntFieldMod overrideEntry = GetOverrideEntry(onHitIntField.GetIdentifier());
                list.Add(
                    overrideEntry != null
                        ? overrideEntry.GetModdedIntField(onHitIntField)
                        : onHitIntField.GetCopy());
            }

            return list;
        }

        private SingleOnHitIntFieldMod GetOverrideEntry(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }

            foreach (IntFieldOverride onHitIntField in m_overrides)
            {
                if (onHitIntField.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return onHitIntField.m_fieldOverride;
                }
            }

            return null;
        }
    }
}