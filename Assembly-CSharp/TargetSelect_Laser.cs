using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_Laser : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public float m_laserRange = 5f;
    public float m_laserWidth = 1f;
    public int m_maxTargets;
    [Separator("AoE around start")]
    public float m_aoeRadiusAroundStart;
    [Separator("Sequences")]
    public GameObject m_castSequencePrefab;
    public GameObject m_aoeAtStartSequencePrefab;

    private TargetSelectMod_Laser m_targetSelMod;

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
                   ContextKeys.s_HitOrder.GetName(),
                   "on every non-caster hit actor, order in which they are hit in laser")
               + GetContextUsageStr(
                   ContextKeys.s_DistFromStart.GetName(),
                   "on every non-caster hit actor, distance from caster");
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(ContextKeys.s_HitOrder.GetName());
        names.Add(ContextKeys.s_DistFromStart.GetName());
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        AbilityUtil_Targeter targeter;
        if (GetAoeRadiusAroundStart() <= 0f)
        {
            targeter = new AbilityUtil_Targeter_Laser(
                ability,
                GetLaserWidth(),
                GetLaserRange(),
                IgnoreLos(),
                GetMaxTargets(),
                IncludeAllies(),
                IncludeCaster());
        }
        else
        {
            targeter = new AbilityUtil_Targeter_ClaymoreSlam(
                ability,
                GetLaserRange(),
                GetLaserWidth(),
                GetMaxTargets(),
                360f,
                GetAoeRadiusAroundStart(),
                0f,
                IgnoreLos());
        }
        targeter.SetAffectedGroups(IncludeEnemies(), IncludeAllies(), IncludeCaster());
        return new List<AbilityUtil_Targeter> { targeter };
    }

    public float GetLaserRange()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_laserRangeMod.GetModifiedValue(m_laserRange)
            : m_laserRange;
    }

    public float GetLaserWidth()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_laserWidthMod.GetModifiedValue(m_laserWidth)
            : m_laserWidth;
    }

    public int GetMaxTargets()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_maxTargetsMod.GetModifiedValue(m_maxTargets)
            : m_maxTargets;
    }

    public float GetAoeRadiusAroundStart()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_aoeRadiusAroundStartMod.GetModifiedValue(m_aoeRadiusAroundStart)
            : m_aoeRadiusAroundStart;
    }

    public override bool CanShowTargeterRangePreview(TargetData[] targetData)
    {
        return true;
    }

    public override float GetTargeterRangePreviewRadius(Ability ability, ActorData caster)
    {
        return GetLaserRange();
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_Laser;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }
}