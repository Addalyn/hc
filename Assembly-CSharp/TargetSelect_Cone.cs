using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_Cone : GenericAbility_TargetSelectBase
{
    [Separator("Input Params")]
    public ConeTargetingInfo m_coneInfo;
    [Separator("Sequences")]
    public GameObject m_coneSequencePrefab;

    private TargetSelectMod_Cone m_targetSelMod;
    private ConeTargetingInfo m_cachedConeInfo;

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
            ContextKeys.s_DistFromStart.GetName(),
            "distance from start of cone position, in squares");
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(ContextKeys.s_DistFromStart.GetName());
    }

    public override void Initialize()
    {
        SetCachedFields();
        ConeTargetingInfo coneInfo = GetConeInfo();
        coneInfo.m_affectsAllies = IncludeAllies();
        coneInfo.m_affectsEnemies = IncludeEnemies();
        coneInfo.m_affectsCaster = IncludeCaster();
        coneInfo.m_penetrateLos = IgnoreLos();
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        ConeTargetingInfo coneInfo = GetConeInfo();
        return new List<AbilityUtil_Targeter>
        {
            new AbilityUtil_Targeter_DirectionCone(
                ability,
                coneInfo.m_widthAngleDeg,
                coneInfo.m_radiusInSquares,
                coneInfo.m_backwardsOffset,
                coneInfo.m_penetrateLos,
                true,
                coneInfo.m_affectsEnemies,
                coneInfo.m_affectsAllies,
                coneInfo.m_affectsCaster)
        };
    }

    private void SetCachedFields()
    {
        m_cachedConeInfo = m_targetSelMod != null
            ? m_targetSelMod.m_coneInfoMod.GetModifiedValue(m_coneInfo)
            : m_coneInfo;
    }

    public ConeTargetingInfo GetConeInfo()
    {
        return m_cachedConeInfo ?? m_coneInfo;
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_Cone;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }
}