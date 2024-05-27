using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_LayerCones : GenericAbility_TargetSelectBase
{
    public delegate int NumActiveLayerDelegate(int maxLayers);

    [Separator("Targeting Properties")]
    public float m_coneWidthAngle = 90f;
    public List<float> m_coneRadiusList;
    [Separator("Sequences")]
    public GameObject m_coneSequencePrefab;
    public NumActiveLayerDelegate m_delegateNumActiveLayers;

    private TargetSelectMod_LayerCones m_targetSelMod;
    private List<float> m_cachedRadiusList = new List<float>();

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
                   ContextKeys.s_Layer.GetName(),
                   "on every hit actor, 0-based index of smallest cone with a hit, with smallest cone first")
               + GetContextUsageStr(
                   ContextKeys.s_LayersActive.GetName(),
                   "Non-actor specific context, number of layers active",
                   false);
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(ContextKeys.s_Layer.GetName());
        names.Add(ContextKeys.s_LayersActive.GetName());
    }

    public override void Initialize()
    {
        base.Initialize();
        m_cachedRadiusList = m_targetSelMod != null && m_targetSelMod.m_useConeRadiusOverrides
            ? new List<float>(m_targetSelMod.m_coneRadiusOverrides)
            : new List<float>(m_coneRadiusList);
        m_cachedRadiusList.Sort();
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        AbilityUtil_Targeter_LayerCones targeter = new AbilityUtil_Targeter_LayerCones(
            ability,
            GetConeWidthAngle(),
            m_cachedRadiusList,
            0f,
            IgnoreLos());
        targeter.SetAffectedGroups(IncludeEnemies(), IncludeAllies(), IncludeCaster());
        return new List<AbilityUtil_Targeter> { targeter };
    }

    public float GetConeWidthAngle()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneWidthAngleMod.GetModifiedValue(m_coneWidthAngle)
            : m_coneWidthAngle;
    }

    public float GetMaxConeRadius()
    {
        float result = 0f;
        int numActiveLayers = GetNumActiveLayers();
        if (numActiveLayers > 0)
        {
            result = m_cachedRadiusList[numActiveLayers - 1];
        }

        return result;
    }

    public int GetNumActiveLayers()
    {
        return m_delegateNumActiveLayers?.Invoke(m_cachedRadiusList.Count) ?? m_cachedRadiusList.Count;
    }

    public int GetLayerCount()
    {
        return m_cachedRadiusList.Count;
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_LayerCones;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }
}