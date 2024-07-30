// ROGUES
// SERVER
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilityContextNamespace
{
    [Serializable]
    public class OnHitDataMod
    {
        // public EffectTemplateFieldListModData m_allHitsEffectTemplateMods; // rogues
        // [Space(20f)]
        public IntFieldListModData m_enemyIntFieldMods;
        public EffectFieldListModData m_enemyEffectMods;
        // public KnockbackFieldListModData m_enemyKnockbackMods; // rogues
        // public EffectTemplateFieldListModData m_enemyHitsEffectTemplateMods; // rogues
        // public CooldownReductionListModData m_enemyCooldownReductionMods; // rogues
        [Space(20f)]
        public IntFieldListModData m_allyIntFieldMods;
        public EffectFieldListModData m_allyEffectMods;
        // public EffectTemplateFieldListModData m_allyHitsEffectTemplateMods; // rogues
        // public CooldownReductionListModData m_allyCooldownReductionMods; // rogues
        // [Space(10f)]
        // public BarrierFieldListModData m_barrierSpawnMods; // rogues
        // public GroundEffectFieldListModData m_groundEffectSpawnMods; // rogues

        public OnHitAuthoredData GetModdedOnHitData(OnHitAuthoredData input)
        {
            return new OnHitAuthoredData
            {
                // m_effectTemplateFields =
                //     m_allHitsEffectTemplateMods.GetModdedEffectTemplateFieldList(input.m_effectTemplateFields), // rogues
                m_enemyHitIntFields = m_enemyIntFieldMods.GetModdedIntFieldList(input.m_enemyHitIntFields),
                m_enemyHitEffectFields = m_enemyEffectMods.GetModdedEffectFieldList(input.m_enemyHitEffectFields),
                // m_enemyHitKnockbackFields =
                //     m_enemyKnockbackMods.GetModdedKnockbackFieldList(input.m_enemyHitKnockbackFields), // rogues
                // m_enemyHitEffectTemplateFields =
                //     m_enemyHitsEffectTemplateMods.GetModdedEffectTemplateFieldList(
                //         input.m_enemyHitEffectTemplateFields), // rogues
                // m_enemyHitCooldownReductionFields =
                //     m_enemyCooldownReductionMods.GetModdedCooldownReductionFieldList(
                //         input.m_enemyHitCooldownReductionFields), // rogues
                m_allyHitIntFields = m_allyIntFieldMods.GetModdedIntFieldList(input.m_allyHitIntFields),
                m_allyHitEffectFields = m_allyEffectMods.GetModdedEffectFieldList(input.m_allyHitEffectFields)
                // m_allyHitEffectTemplateFields =
                //     m_allyHitsEffectTemplateMods.GetModdedEffectTemplateFieldList(
                //         input.m_allyHitEffectTemplateFields), // rogues
                // m_allyHitCooldownReductionFields =
                //     m_allyCooldownReductionMods.GetModdedCooldownReductionFieldList(
                //         input.m_allyHitCooldownReductionFields), // rogues
                // m_barrierSpawnFields = m_barrierSpawnMods.GetModdedBarrierFieldList(input.m_barrierSpawnFields), // rogues
                // m_groundEffectFields =
                //     m_groundEffectSpawnMods.GetModdedGroundEffectFieldList(input.m_groundEffectFields) // rogues
            };
        }

        public string GetInEditorDesc(string header, OnHitAuthoredData baseOnHitData)
        {
            string desc = string.Empty;
            // desc += GetEffectTemplateFieldModDesc(m_allHitsEffectTemplateMods, baseOnHitData?.m_effectTemplateFields, "All Hits Effect Template Field Mods"); // rogues
            desc += GetIntFieldModDesc(m_enemyIntFieldMods, baseOnHitData?.m_enemyHitIntFields, "Enemy Int Field Mods");
            desc += GetEffectFieldModDesc(m_enemyEffectMods, baseOnHitData?.m_enemyHitEffectFields, "Enemy Effect Field Mods");
            // desc += GetKnockbackFieldModDesc(m_enemyKnockbackMods, baseOnHitData?.m_enemyHitKnockbackFields, "Enemy Knockback Field Mods"); // rogues
            // desc += GetEffectTemplateFieldModDesc(m_enemyHitsEffectTemplateMods, baseOnHitData?.m_enemyHitEffectTemplateFields, "Enemy Effect Template Field Mods"); // rogues
            // desc += GetCooldownReductionFieldModDesc(m_enemyCooldownReductionMods, baseOnHitData?.m_enemyHitCooldownReductionFields, "Enemy Cooldown Reduction Field Mods"); // rogues
            desc += GetIntFieldModDesc(m_allyIntFieldMods, baseOnHitData?.m_allyHitIntFields, "Ally Int Field Mods");
            desc += GetEffectFieldModDesc(m_allyEffectMods, baseOnHitData?.m_allyHitEffectFields, "Ally Effect Field Mods");
            // desc += GetEffectTemplateFieldModDesc(m_allyHitsEffectTemplateMods, baseOnHitData?.m_allyHitEffectTemplateFields, "Ally Effect Template Field Mods"); // rogues
            // desc += GetCooldownReductionFieldModDesc(m_allyCooldownReductionMods, baseOnHitData?.m_allyHitCooldownReductionFields, "Ally Cooldown Reduction Field Mods"); // rogues
            // desc += GetBarrierFieldModDesc(m_barrierSpawnMods, baseOnHitData?.m_barrierSpawnFields, "Barrier Spawn Field Mods"); // rogues
            // desc += GetGroundEffectFieldModDesc(m_groundEffectSpawnMods, baseOnHitData?.m_groundEffectFields, "Ground Effect Spawn Field Mods"); // rogues
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

        // rogues
        // public static string GetKnockbackFieldModDesc(
        //     KnockbackFieldListModData knockbackMods,
        //     List<OnHitKnockbackField> baseKnockbackFields,
        //     string header)
        // {
        //     string desc = string.Empty;
        //     if (knockbackMods.m_prependKnockbackFields != null && knockbackMods.m_prependKnockbackFields.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": New entries prepended:</color>\n";
        //         foreach (OnHitKnockbackField onHitKnockbackField in knockbackMods.m_prependKnockbackFields)
        //         {
        //             desc += onHitKnockbackField.GetInEditorDesc();
        //         }
        //     }
        //
        //     if (knockbackMods.m_overrides != null && knockbackMods.m_overrides.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": Override to existing entry:</color>\n";
        //         foreach (KnockbackFieldOverride knockbackFieldOverride in knockbackMods.m_overrides)
        //         {
        //             string identifier = knockbackFieldOverride.GetIdentifier();
        //             if (!string.IsNullOrEmpty(identifier))
        //             {
        //                 OnHitKnockbackField baseField = null;
        //                 if (baseKnockbackFields != null)
        //                 {
        //                     foreach (OnHitKnockbackField field in baseKnockbackFields)
        //                     {
        //                         if (field.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
        //                         {
        //                             baseField = field;
        //                             break;
        //                         }
        //                     }
        //
        //                     if (baseField == null)
        //                     {
        //                         desc = desc
        //                                + "<color=red>Target Identifier "
        //                                + identifier
        //                                + " not found on base on hit data</color>\n";
        //                     }
        //                 }
        //
        //                 desc += "Target Identifier: "
        //                         + InEditorDescHelper.ColoredString(knockbackFieldOverride.m_targetIdentifier, "white")
        //                         + "\n";
        //                 desc += " replacing " + (baseField != null ? baseField.GetInEditorDesc() : "[not found]");
        //                 desc += " with " + knockbackFieldOverride.m_knockbackOverride.GetInEditorDesc();
        //             }
        //         }
        //     }
        //
        //     return desc;
        // }

        // rogues
        // public static string GetBarrierFieldModDesc(
        //     BarrierFieldListModData barrierMods,
        //     List<OnHitBarrierField> baseBarrierFields,
        //     string header)
        // {
        //     string desc = string.Empty;
        //     if (barrierMods.m_prependBarrierFields != null && barrierMods.m_prependBarrierFields.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": New entries prepended:</color>\n";
        //         foreach (OnHitBarrierField onHitBarrierField in barrierMods.m_prependBarrierFields)
        //         {
        //             desc += onHitBarrierField.GetInEditorDesc();
        //         }
        //     }
        //
        //     if (barrierMods.m_overrides != null && barrierMods.m_overrides.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": Override to existing entry:</color>\n";
        //         foreach (OnHitBarrierField onHitFieldOverride in barrierMods.m_overrides)
        //         {
        //             string identifier = onHitFieldOverride.GetIdentifier();
        //             if (!string.IsNullOrEmpty(identifier))
        //             {
        //                 OnHitBarrierField baseOnHitField = null;
        //                 if (baseBarrierFields != null)
        //                 {
        //                     foreach (OnHitBarrierField field in baseBarrierFields)
        //                     {
        //                         if (field.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
        //                         {
        //                             baseOnHitField = field;
        //                             break;
        //                         }
        //                     }
        //
        //                     if (baseOnHitField == null)
        //                     {
        //                         desc += "<color=red>Target Identifier "
        //                                 + identifier
        //                                 + " not found on base on hit data</color>\n";
        //                     }
        //                 }
        //
        //                 desc += "Target Identifier: "
        //                         + InEditorDescHelper.ColoredString(onHitFieldOverride.GetIdentifier(), "white")
        //                         + "\n";
        //                 desc += " replacing " + (baseOnHitField != null ? baseOnHitField.GetInEditorDesc() : "[not found]");
        //                 desc += " with " + onHitFieldOverride.GetInEditorDesc();
        //             }
        //         }
        //     }
        //
        //     return desc;
        // }

        // rogues
        // public static string GetGroundEffectFieldModDesc(
        //     GroundEffectFieldListModData groundEffectMods,
        //     List<OnHitGroundEffectField> baseGroundEffectFields,
        //     string header)
        // {
        //     string desc = string.Empty;
        //     if (groundEffectMods.m_prependEffectFields != null && groundEffectMods.m_prependEffectFields.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": New entries prepended:</color>\n";
        //         foreach (OnHitGroundEffectField onHitGroundEffectField in groundEffectMods.m_prependEffectFields)
        //         {
        //             desc += onHitGroundEffectField.GetInEditorDesc();
        //         }
        //     }
        //
        //     if (groundEffectMods.m_overrides != null && groundEffectMods.m_overrides.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": Override to existing entry:</color>\n";
        //         foreach (GroundEffectFieldOverride groundEffectFieldOverride in groundEffectMods.m_overrides)
        //         {
        //             string identifier = groundEffectFieldOverride.GetIdentifier();
        //             if (!string.IsNullOrEmpty(identifier))
        //             {
        //                 OnHitGroundEffectField baseField = null;
        //                 if (baseGroundEffectFields != null)
        //                 {
        //                     foreach (OnHitGroundEffectField field in baseGroundEffectFields)
        //                     {
        //                         if (field.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
        //                         {
        //                             baseField = field;
        //                             break;
        //                         }
        //                     }
        //
        //                     if (baseField == null)
        //                     {
        //                         desc += "<color=red>Target Identifier "
        //                                 + identifier
        //                                 + " not found on base on hit data</color>\n";
        //                     }
        //                 }
        //
        //                 desc += "Target Identifier: "
        //                         + InEditorDescHelper.ColoredString(groundEffectFieldOverride.m_targetIdentifier, "white")
        //                         + "\n";
        //                 desc += " replacing " + (baseField != null ? baseField.GetInEditorDesc() : "[not found]");
        //                 desc += " with " + groundEffectFieldOverride.m_effectOverride.GetInEditorDesc();
        //             }
        //         }
        //     }
        //
        //     return desc;
        // }

        // rogues
        // public static string GetEffectTemplateFieldModDesc(
        //     EffectTemplateFieldListModData effectMods,
        //     List<OnHitEffectTemplateField> baseEffectTemplateFields,
        //     string header)
        // {
        //     string desc = string.Empty;
        //     if (effectMods.m_prependEffectTemplateFields != null && effectMods.m_prependEffectTemplateFields.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": New entries prepended:</color>\n";
        //         foreach (OnHitEffectTemplateField onHitEffectTemplateField in effectMods.m_prependEffectTemplateFields)
        //         {
        //             desc += onHitEffectTemplateField.GetInEditorDesc(false, null);
        //         }
        //     }
        //
        //     if (effectMods.m_overrides != null && effectMods.m_overrides.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": Override to existing entry:</color>\n";
        //         foreach (EffectTemplateFieldOverride effectTemplateFieldOverride in effectMods.m_overrides)
        //         {
        //             string identifier = effectTemplateFieldOverride.GetIdentifier();
        //             if (!string.IsNullOrEmpty(identifier))
        //             {
        //                 OnHitEffectTemplateField baseField = null;
        //                 if (baseEffectTemplateFields != null)
        //                 {
        //                     foreach (OnHitEffectTemplateField field in baseEffectTemplateFields)
        //                     {
        //                         if (field.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
        //                         {
        //                             baseField = field;
        //                             break;
        //                         }
        //                     }
        //
        //                     if (baseField == null)
        //                     {
        //                         desc += "<color=red>Target Identifier "
        //                                 + identifier
        //                                 + " not found on base on hit data</color>\n";
        //                     }
        //                 }
        //
        //                 desc += "Target Identifier: "
        //                         + InEditorDescHelper.ColoredString(effectTemplateFieldOverride.m_targetIdentifier, "white")
        //                         + "\n";
        //                 desc += effectTemplateFieldOverride.m_effectTemplateOverride.GetInEditorDesc(
        //                     baseField != null,
        //                     baseField);
        //             }
        //         }
        //     }
        //
        //     return desc;
        // }

        // rogues
        // public static string GetCooldownReductionFieldModDesc(
        //     CooldownReductionListModData cooldownMods,
        //     List<OnHitCooldownReductionField> baseCooldownFields,
        //     string header)
        // {
        //     string desc = string.Empty;
        //     if (cooldownMods.m_prependCooldownReductionFields != null
        //         && cooldownMods.m_prependCooldownReductionFields.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": New entries prepended:</color>\n";
        //         foreach (OnHitCooldownReductionField field in cooldownMods.m_prependCooldownReductionFields)
        //         {
        //             desc += field.GetInEditorDesc();
        //         }
        //     }
        //
        //     if (cooldownMods.m_overrides != null && cooldownMods.m_overrides.Count > 0)
        //     {
        //         desc += "<color=cyan>" + header + ": Override to existing entry:</color>\n";
        //         foreach (CooldownReductionFieldOverride cooldownReductionFieldOverride in cooldownMods.m_overrides)
        //         {
        //             string identifier = cooldownReductionFieldOverride.GetIdentifier();
        //             if (!string.IsNullOrEmpty(identifier))
        //             {
        //                 OnHitCooldownReductionField baseField = null;
        //                 if (baseCooldownFields != null)
        //                 {
        //                     foreach (OnHitCooldownReductionField field in baseCooldownFields)
        //                     {
        //                         if (field.GetIdentifier().Equals(identifier, StringComparison.OrdinalIgnoreCase))
        //                         {
        //                             baseField = field;
        //                             break;
        //                         }
        //                     }
        //
        //                     if (baseField == null)
        //                     {
        //                         desc += "<color=red>Target Identifier "
        //                                 + identifier
        //                                 + " not found on base on hit data</color>\n";
        //                     }
        //                 }
        //
        //                 desc += "Target Identifier: "
        //                         + InEditorDescHelper.ColoredString(cooldownReductionFieldOverride.m_targetIdentifier, "white")
        //                         + "\n";
        //                 desc += cooldownReductionFieldOverride.m_cooldownReductionOverride.GetInEditorDesc();
        //             }
        //         }
        //     }
        //
        //     return desc;
        // }

        public void AddTooltipTokens(List<TooltipTokenEntry> tokens, OnHitAuthoredData baseOnHitData)
        {
            if (baseOnHitData != null && m_enemyIntFieldMods != null)
            {
                AddTooltipTokens_IntFields(tokens, m_enemyIntFieldMods, baseOnHitData.m_enemyHitIntFields);
                AddTooltipTokens_EffectFields(tokens, m_enemyEffectMods, baseOnHitData.m_enemyHitEffectFields);
                // AddTooltipTokens_EffectTemplateFields(tokens, m_enemyHitsEffectTemplateMods, baseOnHitData.m_enemyHitEffectTemplateFields); // rogues
                AddTooltipTokens_IntFields(tokens, m_allyIntFieldMods, baseOnHitData.m_allyHitIntFields);
                AddTooltipTokens_EffectFields(tokens, m_allyEffectMods, baseOnHitData.m_allyHitEffectFields);
                // AddTooltipTokens_EffectTemplateFields(tokens, m_allyHitsEffectTemplateMods, baseOnHitData.m_allyHitEffectTemplateFields); // rogues
            }
        }

        // rogues
        // public static void AddTooltipTokens_EffectTemplateFields(
        //     List<TooltipTokenEntry> tokens,
        //     EffectTemplateFieldListModData effectTemplateMods,
        //     List<OnHitEffectTemplateField> baseEffectTemplateFields)
        // {
        //     foreach (OnHitEffectTemplateField onHitEffectTemplateField in
        //              from onHitField in effectTemplateMods.GetModdedEffectTemplateFieldList(baseEffectTemplateFields)
        //              where onHitField.m_effectTemplate != null
        //              select onHitField)
        //     {
        //         SortedSet<EffectTemplate> searched = new SortedSet<EffectTemplate>();
        //         onHitEffectTemplateField.m_effectTemplate.AddTooltipTokens(
        //             searched,
        //             tokens,
        //             onHitEffectTemplateField.m_effectTemplate.name,
        //             false,
        //             null);
        //     }
        // }

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