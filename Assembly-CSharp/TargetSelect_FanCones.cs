using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_FanCones : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public ConeTargetingInfo m_coneInfo;
    [Space(10f)]
    public int m_coneCount = 3;
    [Header("Starting offset, move towards forward/aim direction")]
    public float m_coneStartOffsetInAimDir;
    [Header("Starting offset, move towards left/right")]
    public float m_coneStartOffsetToSides;
    [Header("Starting offset, move towards each cone's direction")]
    public float m_coneStartOffsetInConeDir;
    [Header("-- If Fixed Angle")]
    public float m_angleInBetween = 10f;
    [Header("-- If Interpolating Angle")]
    public bool m_changeAngleByCursorDistance = true;
    public float m_targeterMinAngle;
    public float m_targeterMaxAngle = 180f;
    public float m_startAngleOffset;
    [Space(10f)]
    public float m_targeterMinInterpDistance = 0.5f;
    public float m_targeterMaxInterpDistance = 4f;
    [Separator("Sequences")]
    public GameObject m_castSequencePrefab;

    private TargetSelectMod_FanCones m_targetSelMod;
    private ConeTargetingInfo m_cachedConeInfo;

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
            ContextKeys.s_HitCount.GetName(),
            "on every hit actor, number of cone hits on target");
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(ContextKeys.s_HitCount.GetName());
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

    public int GetConeCount()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneCountMod.GetModifiedValue(m_coneCount)
            : m_coneCount;
    }

    public float GetConeStartOffsetInAimDir()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneStartOffsetInAimDirMod.GetModifiedValue(m_coneStartOffsetInAimDir)
            : m_coneStartOffsetInAimDir;
    }

    public float GetConeStartOffsetToSides()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneStartOffsetToSidesMod.GetModifiedValue(m_coneStartOffsetToSides)
            : m_coneStartOffsetToSides;
    }

    public float GetConeStartOffsetInConeDir()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneStartOffsetInConeDirMod.GetModifiedValue(m_coneStartOffsetInConeDir)
            : m_coneStartOffsetInConeDir;
    }

    public float GetAngleInBetween()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_angleInBetweenMod.GetModifiedValue(m_angleInBetween)
            : m_angleInBetween;
    }

    public bool ChangeAngleByCursorDistance()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_changeAngleByCursorDistanceMod.GetModifiedValue(m_changeAngleByCursorDistance)
            : m_changeAngleByCursorDistance;
    }

    public float GetTargeterMinAngle()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_targeterMinAngleMod.GetModifiedValue(m_targeterMinAngle)
            : m_targeterMinAngle;
    }

    public float GetTargeterMaxAngle()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_targeterMaxAngleMod.GetModifiedValue(m_targeterMaxAngle)
            : m_targeterMaxAngle;
    }

    public float GetStartAngleOffset()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_startAngleOffsetMod.GetModifiedValue(m_startAngleOffset)
            : m_startAngleOffset;
    }

    protected virtual bool UseCasterPosForLoS()
    {
        return false;
    }

    protected virtual bool CustomLoS(ActorData actor, ActorData caster)
    {
        return true;
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        AbilityUtil_Targeter_TricksterCones targeter = new AbilityUtil_Targeter_TricksterCones(
            ability,
            GetConeInfo(),
            GetConeCount(),
            GetConeCount,
            GetConeOrigins,
            GetConeDirections,
            GetFreePosForAim,
            false,
            UseCasterPosForLoS())
        {
            m_customDamageOriginDelegate = GetDamageOriginForTargeter
        };
        return new List<AbilityUtil_Targeter> { targeter };
    }

    private Vector3 GetDamageOriginForTargeter(
        AbilityTarget currentTarget,
        Vector3 defaultOrigin,
        ActorData actorToAdd,
        ActorData caster)
    {
        return caster.GetFreePos();
    }

    public Vector3 GetFreePosForAim(AbilityTarget currentTarget, ActorData caster)
    {
        return currentTarget.FreePos;
    }

    public virtual List<Vector3> GetConeOrigins(AbilityTarget currentTarget, Vector3 targeterFreePos, ActorData caster)
    {
        List<Vector3> list = new List<Vector3>();
        Vector3 losCheckPos = caster.GetLoSCheckPos();
        Vector3 aimDirection = currentTarget.AimDirection;
        Vector3 normalized = Vector3.Cross(aimDirection, Vector3.up).normalized;
        int coneCount = GetConeCount();
        int halfConeCount = coneCount / 2;
        bool evenCones = coneCount % 2 == 0;
        float aimDirOffset = GetConeStartOffsetInAimDir() * Board.SquareSizeStatic;
        float sideOffsetDir = GetConeStartOffsetToSides() * Board.SquareSizeStatic;
        for (int i = 0; i < coneCount; i++)
        {
            Vector3 b = Vector3.zero;
            if (aimDirOffset != 0f)
            {
                b = aimDirOffset * aimDirection;
            }

            if (sideOffsetDir > 0f)
            {
                if (evenCones)
                {
                    if (i < halfConeCount)
                    {
                        b -= (halfConeCount - i) * sideOffsetDir * normalized;
                    }
                    else
                    {
                        b += (i - halfConeCount + 1) * sideOffsetDir * normalized;
                    }
                }
                else if (i < halfConeCount)
                {
                    b -= (halfConeCount - i) * sideOffsetDir * normalized;
                }
                else if (i > halfConeCount)
                {
                    b += (i - halfConeCount) * sideOffsetDir * normalized;
                }
            }

            list.Add(losCheckPos + b);
        }

        if (GetConeStartOffsetInConeDir() > 0f)
        {
            List<Vector3> coneDirections = GetConeDirections(currentTarget, targeterFreePos, caster);
            float d = GetConeStartOffsetInConeDir() * Board.SquareSizeStatic;
            for (int i = 0; i < coneDirections.Count; i++)
            {
                list[i] += d * coneDirections[i];
            }
        }

        return list;
    }

    public virtual List<Vector3> GetConeDirections(
        AbilityTarget currentTarget,
        Vector3 targeterFreePos,
        ActorData caster)
    {
        List<Vector3> list = new List<Vector3>();
        float angleInBetween = GetAngleInBetween();
        int coneCount = GetConeCount();
        if (ChangeAngleByCursorDistance())
        {
            float angleTotal = coneCount <= 1
                ? 0f
                : AbilityCommon_FanLaser.CalculateFanAngleDegrees(
                    currentTarget,
                    caster,
                    GetTargeterMinAngle(),
                    GetTargeterMaxAngle(),
                    m_targeterMinInterpDistance,
                    m_targeterMaxInterpDistance,
                    0f);

            angleInBetween = coneCount > 1
                ? angleTotal / (coneCount - 1)
                : 0f;
        }

        float aimAngle = VectorUtils.HorizontalAngle_Deg(currentTarget.AimDirection) + GetStartAngleOffset();
        float startAngle = aimAngle - 0.5f * (coneCount - 1) * angleInBetween;
        for (int i = 0; i < coneCount; i++)
        {
            list.Add(VectorUtils.AngleDegreesToVector(startAngle + i * angleInBetween));
        }

        return list;
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_FanCones;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }
}