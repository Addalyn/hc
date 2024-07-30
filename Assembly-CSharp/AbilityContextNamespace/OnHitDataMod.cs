using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilityContextNamespace
{
    [Serializable]
    public class OnHitDataMod
    {
        public IntFieldListModData m_enemyIntFieldMods;
        public EffectFieldListModData m_enemyEffectMods;
        [Space(20f)]
        public IntFieldListModData m_allyIntFieldMods;
        public EffectFieldListModData m_allyEffectMods;

        public OnHitAuthoredData GetModdedOnHitData(OnHitAuthoredData input)
        {
            return new OnHitAuthoredData
            {
                m_enemyHitIntFields = m_enemyIntFieldMods.GetModdedIntFieldList(input.m_enemyHitIntFields),
                m_enemyHitEffectFields = m_enemyEffectMods.GetModdedEffectFieldList(input.m_enemyHitEffectFields),
                m_allyHitIntFields = m_allyIntFieldMods.GetModdedIntFieldList(input.m_allyHitIntFields),
                m_allyHitEffectFields = m_allyEffectMods.GetModdedEffectFieldList(input.m_allyHitEffectFields)
            };
        }

        public string GetInEditorDesc(string header, OnHitAuthoredData baseOnHitData)
        {
            string desc = GetIntFieldModDesc(m_enemyIntFieldMods, baseOnHitData?.m_enemyHitIntFields, "Enemy Int Field Mods");
            desc += GetEffectFieldModDesc(m_enemyEffectMods, baseOnHitData?.m_enemyHitEffectFields, "Enemy Effect Field Mods");
            desc += GetIntFieldModDesc(m_allyIntFieldMods, baseOnHitData?.m_allyHitIntFields, "Ally Int Field Mods");
            desc += GetEffectFieldModDesc(m_allyEffectMods, baseOnHitData?.m_allyHitEffectFields, "Ally Effect Field Mods");
            if (desc.Length > 0)
            {
                desc = InEditorDescHelper.ColoredString(header, "yellow") + "\n" + desc + "\n";
            }

            return desc;
        }

        public static string GetIntFieldModDesc(
            IntFieldListModData intMods,
            List<OnHitIntField> baseIntFields,
            string header)
        {
            string desc = string.Empty;
            if (intMods.m_prependIntFields != null && intMods.m_prependIntFields.Count > 0)
            {
                desc += "<color=cyan>" + header + ": New entries prepended:</color>\n";
                foreach (OnHitIntField onHitIntField in intMods.m_prependIntFields)
                {
                    desc += onHitIntField.GetInEditorDesc();
                }
            }

            if (intMods.m_overrides != null && intMods.m_overrides.Count > 0)
            {
                desc += "<color=cyan>" + header + ": Override to existing entry:</color>\n";
                foreach (IntFieldOverride intFieldOverride in intMods.m_overrides)
                {
                    string identifier = intFieldOverride.GetIdentifier();
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        desc += "Target Identifier: "
                                + InEditorDescHelper.ColoredString(intFieldOverride.m_targetIdentifier, "white")
                                + "\n";
                        if (baseIntFields != null)
                        {
                            bool isFound = false;
                            foreach (OnHitIntField field in baseIntFields)
                            {
                                if (field.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
                                {
                                    isFound = true;
                                    desc += intFieldOverride.m_fieldOverride.GetInEditorDesc(field);
                                    break;
                                }
                            }

                            if (!isFound)
                            {
                                desc += "<color=red>Target Identifier "
                                        + identifier
                                        + " not found on base on hit data</color>\n";
                            }
                        }
                    }
                }
            }

            return desc;
        }

        public static string GetEffectFieldModDesc(
            EffectFieldListModData effectMods,
            List<OnHitEffecField> baseEffectFields,
            string header)
        {
            string desc = string.Empty;
            if (effectMods.m_prependEffectFields != null && effectMods.m_prependEffectFields.Count > 0)
            {
                desc += "<color=cyan>" + header + ": New entries prepended:</color>\n";
                foreach (OnHitEffecField onHitEffecField in effectMods.m_prependEffectFields)
                {
                    desc += onHitEffecField.GetInEditorDesc(false, null);
                }
            }

            if (effectMods.m_overrides != null && effectMods.m_overrides.Count > 0)
            {
                desc += "<color=cyan>" + header + ": Override to existing entry:</color>\n";
                foreach (EffectFieldOverride effectFieldOverride in effectMods.m_overrides)
                {
                    string identifier = effectFieldOverride.GetIdentifier();
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        OnHitEffecField baseEffectField = null;
                        if (baseEffectFields != null)
                        {
                            foreach (OnHitEffecField field in baseEffectFields)
                            {
                                if (field.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
                                {
                                    baseEffectField = field;
                                    break;
                                }
                            }

                            if (baseEffectField == null)
                            {
                                desc += "<color=red>Target Identifier " + identifier
                                                                        + " not found on base on hit data</color>\n";
                            }
                        }

                        desc += "Target Identifier: "
                                + InEditorDescHelper.ColoredString(effectFieldOverride.m_targetIdentifier, "white")
                                + "\n";
                        desc += effectFieldOverride.m_effectOverride.GetInEditorDesc(baseEffectField != null, baseEffectField);
                    }
                }
            }

            return desc;
        }

        public void AddTooltipTokens(List<TooltipTokenEntry> tokens, OnHitAuthoredData baseOnHitData)
        {
            if (baseOnHitData != null && m_enemyIntFieldMods != null)
            {
                AddTooltipTokens_IntFields(tokens, m_enemyIntFieldMods, baseOnHitData.m_enemyHitIntFields);
                AddTooltipTokens_EffectFields(tokens, m_enemyEffectMods, baseOnHitData.m_enemyHitEffectFields);
                AddTooltipTokens_IntFields(tokens, m_allyIntFieldMods, baseOnHitData.m_allyHitIntFields);
                AddTooltipTokens_EffectFields(tokens, m_allyEffectMods, baseOnHitData.m_allyHitEffectFields);
            }
        }

        public static void AddTooltipTokens_IntFields(
            List<TooltipTokenEntry> tokens,
            IntFieldListModData intMods,
            List<OnHitIntField> baseIntFields)
        {
            if (intMods.m_prependIntFields != null)
            {
                foreach (OnHitIntField onHitIntField in intMods.m_prependIntFields)
                {
                    if (!string.IsNullOrEmpty(onHitIntField.GetIdentifier()))
                    {
                        onHitIntField.AddTooltipTokens(tokens);
                    }
                }
            }

            if (intMods.m_overrides != null)
            {
                foreach (IntFieldOverride intFieldOverride in intMods.m_overrides)
                {
                    string identifier = intFieldOverride.GetIdentifier();
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        OnHitIntField baseIntField = null;
                        foreach (OnHitIntField field in baseIntFields)
                        {
                            if (identifier.Equals(field.GetIdentifier(), StringComparison.OrdinalIgnoreCase))
                            {
                                baseIntField = field;
                                break;
                            }
                        }

                        if (baseIntField != null)
                        {
                            intFieldOverride.m_fieldOverride.AddTooltipTokens(tokens, baseIntField, identifier);
                        }
                    }
                }
            }
        }

        public static void AddTooltipTokens_EffectFields(
            List<TooltipTokenEntry> tokens,
            EffectFieldListModData effectMods,
            List<OnHitEffecField> baseEffectFields)
        {
            if (effectMods.m_prependEffectFields != null)
            {
                foreach (OnHitEffecField onHitEffecField in effectMods.m_prependEffectFields)
                {
                    if (!string.IsNullOrEmpty(onHitEffecField.GetIdentifier()))
                    {
                        onHitEffecField.AddTooltipTokens(tokens, false, null);
                    }
                }
            }

            if (effectMods.m_overrides != null)
            {
                foreach (EffectFieldOverride effectFieldOverride in effectMods.m_overrides)
                {
                    string identifier = effectFieldOverride.GetIdentifier();
                    if (!string.IsNullOrEmpty(identifier))
                    {
                        OnHitEffecField baseEffectField = null;
                        foreach (OnHitEffecField field in baseEffectFields)
                        {
                            if (identifier.Equals(field.GetIdentifier(), StringComparison.OrdinalIgnoreCase))
                            {
                                baseEffectField = field;
                                break;
                            }
                        }

                        if (baseEffectField != null)
                        {
                            effectFieldOverride.m_effectOverride.AddTooltipTokens(
                                tokens,
                                true,
                                baseEffectField,
                                identifier);
                        }
                    }
                }
            }
        }
    }
}