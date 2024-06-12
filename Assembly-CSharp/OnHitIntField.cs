using AbilityContextNamespace;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OnHitIntField
{
    public enum HitType
    {
        Damage,
        Healing,
        EnergyChange
    }

    [Header("-- used to match mod data and generate tooltip tokens. Not case sensitive, but should be unique within ability")]
    public string m_identifier = string.Empty;
    public TargetFilterConditions m_conditions;
    public HitType m_hitType;
    public int m_baseValue;
    public int m_minValue;
    public int m_maxValue;
    [Header("-- Values taken from context vars and added to base value")]
    public int m_baseAddTotalMinValue;
    public int m_baseAddTotalMaxValue;
    public List<NumericContextOperand> m_baseAddModifiers;

    public string GetIdentifier()
    {
        return m_identifier.Trim();
    }

    public OnHitIntField GetCopy()
    {
        OnHitIntField onHitIntField = MemberwiseClone() as OnHitIntField;
        onHitIntField.m_conditions = m_conditions.GetCopy();
        onHitIntField.m_baseAddModifiers = new List<NumericContextOperand>();
        for (int i = 0; i < m_baseAddModifiers.Count; i++)
        {
            NumericContextOperand copy = m_baseAddModifiers[i].GetCopy();
            onHitIntField.m_baseAddModifiers.Add(copy);
        }

        return onHitIntField;
    }

    public int CalcValue(ActorHitContext hitContext, ContextVars abilityContext)
    {
        int baseValue = m_baseValue;
        int num = 0;
        foreach (NumericContextOperand mod in m_baseAddModifiers)
        {
            int contextKey = mod.GetContextKey();
            bool hasContextValue = false;
            float contextValue = 0f;
            if (mod.m_nonActorSpecificContext)
            {
                if (abilityContext.HasVarInt(contextKey))
                {
                    contextValue = abilityContext.GetValueInt(contextKey);
                    hasContextValue = true;
                }
                else if (abilityContext.HasVarFloat(contextKey))
                {
                    contextValue = abilityContext.GetValueFloat(contextKey);
                    hasContextValue = true;
                }
            }
            else if (hitContext.m_contextVars.HasVarInt(contextKey))
            {
                contextValue = hitContext.m_contextVars.GetValueInt(contextKey);
                hasContextValue = true;
            }
            else if (hitContext.m_contextVars.HasVarFloat(contextKey))
            {
                contextValue = hitContext.m_contextVars.GetValueFloat(contextKey);
                hasContextValue = true;
            }

            if (!hasContextValue)
            {
                continue;
            }

            float modifiedValue = mod.m_modifier.GetModifiedValue(contextValue);
            if (mod.m_additionalModifiers != null)
            {
                foreach (AbilityModPropertyFloat additionalMod in mod.m_additionalModifiers)
                {
                    modifiedValue = additionalMod.GetModifiedValue(modifiedValue);
                }
            }

            num += Mathf.RoundToInt(modifiedValue);
        }

        if (num < m_baseAddTotalMinValue)
        {
            num = m_baseAddTotalMinValue;
        }
        else if (num > m_baseAddTotalMaxValue)
        {
            if (m_baseAddTotalMaxValue > 0)
            {
                num = m_baseAddTotalMaxValue;
            }
        }

        baseValue += num;
        if (baseValue < m_minValue)
        {
            baseValue = m_minValue;
        }
        else if (baseValue > m_maxValue && m_maxValue > 0)
        {
            baseValue = m_maxValue;
        }

        return baseValue;
    }

    public void AddTooltipTokens(List<TooltipTokenEntry> tokens)
    {
        string identifier = GetIdentifier();
        if (!string.IsNullOrEmpty(identifier))
        {
            TooltipTokenHelper.AddTokenInt(tokens, identifier + "_Base", m_baseValue, string.Empty);
            TooltipTokenHelper.AddTokenInt(tokens, identifier + "_Min", m_minValue, string.Empty);
            TooltipTokenHelper.AddTokenInt(tokens, identifier + "_Max", m_maxValue, string.Empty);
            TooltipTokenHelper.AddTokenInt(tokens, identifier + "_BaseAddMin", m_baseAddTotalMinValue, string.Empty);
            TooltipTokenHelper.AddTokenInt(tokens, identifier + "_BaseAddMax", m_baseAddTotalMaxValue, string.Empty);
        }
    }

    public string GetInEditorDesc()
    {
        string desc = "Field Type < " + InEditorDescHelper.ColoredString(m_hitType.ToString()) + " >\n";
        if (!string.IsNullOrEmpty(m_identifier))
        {
            desc += "Identifier: " + InEditorDescHelper.ColoredString(m_identifier, "white") + "\n";
        }

        desc += "Conditions:\n" + m_conditions.GetInEditorDesc("    ");
        desc += "BaseValue= " + InEditorDescHelper.ColoredString(m_baseValue) + "\n";
        if (m_minValue > 0)
        {
            desc += "MinValue= " + InEditorDescHelper.ColoredString(m_minValue) + "\n";
        }

        if (m_maxValue > 0)
        {
            desc += "MaxValue= " + InEditorDescHelper.ColoredString(m_maxValue) + "\n";
        }

        if (m_baseAddModifiers.Count > 0)
        {
            desc += "+ Base Add Modifiers\n";
            foreach (NumericContextOperand mod in m_baseAddModifiers)
            {
                desc += mod.GetInEditorDesc("    ");
            }
        }

        return desc;
    }
}