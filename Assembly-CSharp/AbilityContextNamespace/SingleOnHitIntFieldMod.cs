// ROGUES
// SERVER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilityContextNamespace
{
    [Serializable]
    public class SingleOnHitIntFieldMod
    {
        [Header("-- Condition override")]
        public bool m_useConditionOverride;
        public TargetFilterConditions m_conditionOverride;
        [Header("-- Base value and its limits")]
        public AbilityModPropertyInt m_baseValueMod; // removed in rogues
        public AbilityModPropertyInt m_minValueMod; // AbilityModPropertyFloat in rogues
        public AbilityModPropertyInt m_maxValueMod; // AbilityModPropertyFloat in rogues
        [Header("-- Limits on Base Add Total")]
        public AbilityModPropertyInt m_baseAddTotalMinValueMod; // AbilityModPropertyFloat in rogues
        public AbilityModPropertyInt m_baseAddTotalMaxValueMod; // AbilityModPropertyFloat in rogues
        [Header("-- Whether to override base add modifiers")]
        public bool m_useBaseAddModifierOverrides;
        public List<NumericContextOperand> m_baseAddModifierOverrides;

        public OnHitIntField GetModdedIntField(OnHitIntField baseIntField)
        {
            OnHitIntField copy = baseIntField.GetCopy();
            if (m_useConditionOverride)
            {
                copy.m_conditions = m_conditionOverride.GetCopy();
            }

            copy.m_baseValue = m_baseValueMod.GetModifiedValue(baseIntField.m_baseValue); // removed in rogues
            copy.m_minValue = m_minValueMod.GetModifiedValue(baseIntField.m_minValue);
            copy.m_maxValue = m_maxValueMod.GetModifiedValue(baseIntField.m_maxValue);
            copy.m_baseAddTotalMinValue = m_baseAddTotalMinValueMod.GetModifiedValue(baseIntField.m_baseAddTotalMinValue);
            copy.m_baseAddTotalMaxValue = m_baseAddTotalMaxValueMod.GetModifiedValue(baseIntField.m_baseAddTotalMaxValue);
            if (m_useBaseAddModifierOverrides)
            {
                copy.m_baseAddModifiers = new List<NumericContextOperand>();
                foreach (NumericContextOperand mod in m_baseAddModifierOverrides)
                {
                    copy.m_baseAddModifiers.Add(mod.GetCopy());
                }
            }

            return copy;
        }

        public string GetInEditorDesc(OnHitIntField baseIntField)
        {
            string desc = string.Empty;
            if (baseIntField != null)
            {
                if (m_useConditionOverride)
                {
                    desc += "* Using Condition override *\n";
                    desc += m_conditionOverride.GetInEditorDesc("    ");
                }

                desc += AbilityModHelper.GetModPropertyDesc(m_baseValueMod, "[BaseValue]", true, baseIntField.m_baseValue); // removed in rogues
                desc += AbilityModHelper.GetModPropertyDesc(m_minValueMod, "[MinValue]", true, baseIntField.m_minValue);
                desc += AbilityModHelper.GetModPropertyDesc(m_maxValueMod, "[MaxValue]", true, baseIntField.m_maxValue);
                desc += AbilityModHelper.GetModPropertyDesc(m_baseAddTotalMinValueMod, "[BaseAddTotalMinValue]", true, baseIntField.m_baseAddTotalMinValue);
                desc += AbilityModHelper.GetModPropertyDesc(m_baseAddTotalMaxValueMod, "[BaseAddTotalMaxValue]", true, baseIntField.m_baseAddTotalMaxValue);
                if (m_useBaseAddModifierOverrides)
                {
                    desc += "* Using Base Add Modifier Overrides *\n";
                    if (m_baseAddModifierOverrides.Count > 0)
                    {
                        desc += "+ Base Add Modifiers\n";
                        foreach (NumericContextOperand numericContextOperand in m_baseAddModifierOverrides)
                        {
                            desc += numericContextOperand.GetInEditorDesc("    ");
                        }
                    }
                }
            }

            return desc;
        }

        public void AddTooltipTokens(List<TooltipTokenEntry> tokens, OnHitIntField baseIntField, string name)
        {
            if (baseIntField == null)
            {
                return;
            }

            AbilityMod.AddToken(tokens, m_baseValueMod, name + "_Base", string.Empty, baseIntField.m_baseValue); // removed in rogues
            AbilityMod.AddToken(tokens, m_minValueMod, name + "_Min", string.Empty, baseIntField.m_minValue);
            AbilityMod.AddToken(tokens, m_maxValueMod, name + "_Max", string.Empty, baseIntField.m_maxValue);
            AbilityMod.AddToken(tokens, m_baseAddTotalMinValueMod, name + "_BaseAddTotalMin", string.Empty, baseIntField.m_baseAddTotalMinValue);
            AbilityMod.AddToken(tokens, m_baseAddTotalMaxValueMod, name + "_BaseAddTotalMax", string.Empty, baseIntField.m_baseAddTotalMaxValue);

            if (!m_useBaseAddModifierOverrides || m_baseAddModifierOverrides == null)
            {
                return;
            }

            for (int i = 0; i < m_baseAddModifierOverrides.Count; i++)
            {
                NumericContextOperand numericContextOperand = m_baseAddModifierOverrides[i];
                if (!numericContextOperand.m_contextName.IsNullOrEmpty())
                {
                    int val = Mathf.RoundToInt(numericContextOperand.m_modifier.value);
                    if (val > 0)
                    {
                        tokens.Add(new TooltipTokenInt(name + "_Add_" + i + "_Main", string.Empty, val));
                    }

                    if (numericContextOperand.m_additionalModifiers != null)
                    {
                        for (int j = 0; j < numericContextOperand.m_additionalModifiers.Count; j++)
                        {
                            int additionalVal = Mathf.RoundToInt(numericContextOperand.m_additionalModifiers[j].value);
                            if (additionalVal > 0)
                            {
                                tokens.Add(
                                    new TooltipTokenInt(
                                        name + "_Add_" + i + "_Extra_" + j,
                                        string.Empty,
                                        additionalVal));
                            }
                        }
                    }
                }
            }
        }
    }
}