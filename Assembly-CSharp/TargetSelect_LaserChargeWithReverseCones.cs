using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_LaserChargeWithReverseCones : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public float m_laserRange = 5f;
    public float m_laserWidth = 1f;
    [Header("Cone Properties")]
    public ConeTargetingInfo m_coneInfo;
    [Space(10f)]
    public int m_coneCount = 3;
    public float m_coneStartOffset;
    public float m_perConeHorizontalOffset;
    public float m_angleInBetween = 10f;
    [Separator("Sequences")]
    public GameObject m_castSequencePrefab;
    public GameObject m_coneSequencePrefab;

    private const string c_directChargeHit = "DirectChargeHit";
    public static ContextNameKeyPair s_cvarDirectChargeHit = new ContextNameKeyPair(c_directChargeHit);

    private TargetSelectMod_LaserChargeWithReverseCones m_targetSelMod;
    private ConeTargetingInfo m_cachedConeInfo;

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
            s_cvarDirectChargeHit.GetName(),
            "whether this is a direct charge hit or not (if not, it's a cone hit)");
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(c_directChargeHit);
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
        return new List<AbilityUtil_Targeter>
        {
            new AbilityUtil_Targeter_LaserChargeReverseCones(
                ability,
                GetLaserWidth(),
                GetLaserRange(),
                GetConeInfo(),
                GetConeCount(),
                GetConeStartOffset(),
                GetPerConeHorizontalOffset(),
                GetAngleInBetween(),
                GetConeOrigins,
                GetConeDirections)
            {
                m_coneLosCheckDelegate = CustomLosForCone
            }
        };
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_LaserChargeWithReverseCones;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }

    private void SetCachedFields()
    {
        m_cachedConeInfo = m_targetSelMod != null
            ? m_targetSelMod.m_coneInfoMod.GetModifiedValue(m_coneInfo)
            : m_coneInfo;
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

    public float GetConeStartOffset()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneStartOffsetMod.GetModifiedValue(m_coneStartOffset)
            : m_coneStartOffset;
    }

    public float GetPerConeHorizontalOffset()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_perConeHorizontalOffsetMod.GetModifiedValue(m_perConeHorizontalOffset)
            : m_perConeHorizontalOffset;
    }

    public float GetAngleInBetween()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_angleInBetweenMod.GetModifiedValue(m_angleInBetween)
            : m_angleInBetween;
    }

    public override ActorData.MovementType GetMovementType()
    {
        return ActorData.MovementType.Charge;
    }

    protected List<Vector3> GetConeOrigins(AbilityTarget currentTarget, Vector3 targeterFreePos, ActorData caster)
    {
        List<Vector3> list = new List<Vector3>();
        List<Vector3> coneDirections = GetConeDirections(currentTarget, targeterFreePos, caster);

        Vector3 reverseConeDir = -currentTarget.AimDirection;
        reverseConeDir.Normalize();
        Vector3 reverseConeRight = Vector3.Cross(reverseConeDir, Vector3.up).normalized;

        float coneStartOffset = GetConeStartOffset() * Board.SquareSizeStatic;
        Vector3 coneEndPos = targeterFreePos + coneStartOffset * reverseConeDir;
        for (int i = 0; i < coneDirections.Count; i++)
        {
            float offset = GetPerConeHorizontalOffset() * (i - coneDirections.Count / 2);
            list.Add(coneEndPos
                     + reverseConeRight * offset
                     - GetConeInfo().m_radiusInSquares * Board.SquareSizeStatic * coneDirections[i]);
        }

        return list;
    }

    public virtual List<Vector3> GetConeDirections(
        AbilityTarget currentTarget,
        Vector3 targeterFreePos,
        ActorData caster)
    {
        List<Vector3> list = new List<Vector3>();
        int coneCount = GetConeCount();
        float angleInBetween = GetAngleInBetween();
        float aimAngle = VectorUtils.HorizontalAngle_Deg(currentTarget.AimDirection);
        float startAngle = aimAngle + 0.5f * (coneCount - 1) * angleInBetween;
        for (int i = 0; i < coneCount; i++)
        {
            list.Add(-VectorUtils.AngleDegreesToVector(startAngle - i * angleInBetween));
        }

        return list;
    }

    public static bool CustomLosForCone(
        ActorData actor,
        ActorData caster,
        Vector3 chargeEndPos,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        BoardSquare chargeEndSquare = Board.Get().GetSquareFromVec3(chargeEndPos);
        return chargeEndSquare != null && AreaEffectUtils.SquaresHaveLoSForAbilities(
            chargeEndSquare,
            actor.GetCurrentBoardSquare(),
            caster,
            true,
            nonActorTargetInfo);
    }
}