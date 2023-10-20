using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_ConeOrLaser : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public float m_coneDistThreshold = 4f;
    [Header("  Targeting: For Cone")]
    public ConeTargetingInfo m_coneInfo;
    [Header("  Targeting: For Laser")]
    public LaserTargetingInfo m_laserInfo;
    [Separator("Sequences")]
    public GameObject m_coneSequencePrefab;
    public GameObject m_laserSequencePrefab;

    public static ContextNameKeyPair s_cvarInCone = new ContextNameKeyPair("InCone");

    private TargetSelectMod_ConeOrLaser m_targetSelMod;
    private ConeTargetingInfo m_cachedConeInfo;
    private LaserTargetingInfo m_cachedLaserInfo;

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
                   ContextKeys.s_DistFromStart.GetName(),
                   "distance from start of cone position, in squares")
               + GetContextUsageStr(
                   s_cvarInCone.GetName(),
                   "Whether the target hit is in cone")
               + GetContextUsageStr(
                   ContextKeys.s_AngleFromCenter.GetName(),
                   "angle from center of cone");
    }

    public override void ListContextNamesForEditor(List<string> keys)
    {
        keys.Add(ContextKeys.s_DistFromStart.GetName());
        keys.Add(s_cvarInCone.GetName());
        keys.Add(ContextKeys.s_AngleFromCenter.GetName());
    }

    public override void Initialize()
    {
        base.Initialize();
        SetCachedFields();
        ConeTargetingInfo coneInfo = GetConeInfo();
        coneInfo.m_affectsEnemies = IncludeEnemies();
        coneInfo.m_affectsAllies = IncludeAllies();
        coneInfo.m_affectsCaster = IncludeCaster();
        LaserTargetingInfo laserInfo = GetLaserInfo();
        laserInfo.affectsEnemies = IncludeEnemies();
        laserInfo.affectsAllies = IncludeAllies();
        laserInfo.affectsCaster = IncludeCaster();
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        return new List<AbilityUtil_Targeter>
        {
            new AbilityUtil_Targeter_ConeOrLaser(
                ability,
                GetConeInfo(),
                GetLaserInfo(),
                GetConeDistThreshold())
        };
    }

    public bool ShouldUseCone(Vector3 freePos, ActorData caster)
    {
        Vector3 vector = freePos - caster.GetFreePos();
        vector.y = 0f;
        return vector.magnitude <= GetConeDistThreshold();
    }

    private void SetCachedFields()
    {
        m_cachedConeInfo = m_targetSelMod != null
            ? m_targetSelMod.m_coneInfoMod.GetModifiedValue(m_coneInfo)
            : m_coneInfo;
        m_cachedLaserInfo = m_targetSelMod != null
            ? m_targetSelMod.m_laserInfoMod.GetModifiedValue(m_laserInfo)
            : m_laserInfo;
    }

    public float GetConeDistThreshold()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneDistThresholdMod.GetModifiedValue(m_coneDistThreshold)
            : m_coneDistThreshold;
    }

    public ConeTargetingInfo GetConeInfo()
    {
        return m_cachedConeInfo ?? m_coneInfo;
    }

    public LaserTargetingInfo GetLaserInfo()
    {
        return m_cachedLaserInfo ?? m_laserInfo;
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_ConeOrLaser;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }

    //rogues
    public override void CalcHitTargets(
        List<AbilityTarget> targets,
        ActorData caster,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        ResetContextData();
        base.CalcHitTargets(targets, caster, nonActorTargetInfo);
        bool useCone = ShouldUseCone(targets[0].FreePos, caster);
        Vector3 loSCheckPos = caster.GetLoSCheckPos();
        foreach (ActorData actor in GetHitActors(targets, caster, useCone, nonActorTargetInfo, out _))
        {
            AddHitActor(actor, loSCheckPos);
            SetActorContext(actor, s_cvarInCone.GetKey(), useCone ? 1 : 0);
        }
    }

    //rogues
    private List<ActorData> GetHitActors(
        List<AbilityTarget> targets,
        ActorData caster,
        bool useCone,
        List<NonActorTargetInfo> nonActorTargetInfo,
        out Vector3 endPosIfLaser)
    {
        Vector3 loSCheckPos = caster.GetLoSCheckPos();
        float coneCenterAngleDegrees = VectorUtils.HorizontalAngle_Deg(targets[0].AimDirection);
        List<Team> affectedTeams = m_coneInfo.GetAffectedTeams(caster);
        List<ActorData> result;
        if (useCone)
        {
            result = AreaEffectUtils.GetActorsInCone(loSCheckPos,
                coneCenterAngleDegrees,
                m_coneInfo.m_widthAngleDeg,
                m_coneInfo.m_radiusInSquares,
                m_coneInfo.m_backwardsOffset,
                m_coneInfo.m_penetrateLos,
                caster,
                affectedTeams,
                nonActorTargetInfo);
            endPosIfLaser = targets[0].FreePos;
        }
        else
        {
            VectorUtils.LaserCoords laserCoords;
            laserCoords.start = loSCheckPos;
            result = AreaEffectUtils.GetActorsInLaser(
                laserCoords.start,
                targets[0].AimDirection,
                m_laserInfo.range,
                m_laserInfo.width,
                caster,
                affectedTeams,
                m_laserInfo.penetrateLos,
                m_laserInfo.maxTargets,
                false,
                true,
                out laserCoords.end,
                nonActorTargetInfo);
            endPosIfLaser = laserCoords.end;
        }

        return result;
    }

    //rogues
    public override List<ServerClientUtils.SequenceStartData> CreateSequenceStartData(
        List<AbilityTarget> targets,
        ActorData caster,
        ServerAbilityUtils.AbilityRunData additionalData,
        Sequence.IExtraSequenceParams[] extraSequenceParams = null)
    {
        bool useCone = ShouldUseCone(targets[0].FreePos, caster);
        GetHitActors(targets, caster, useCone, null, out Vector3 targetPos);
        List<Sequence.IExtraSequenceParams> sequenceParams = new List<Sequence.IExtraSequenceParams>();
        if (extraSequenceParams != null)
        {
            sequenceParams.AddRange(extraSequenceParams);
        }

        if (useCone)
        {
            sequenceParams.Add(new BlasterStretchConeSequence.ExtraParams
            {
                forwardAngle = VectorUtils.HorizontalAngle_Deg(targets[0].AimDirection),
                angleInDegrees = m_coneInfo.m_widthAngleDeg,
                lengthInSquares = m_coneInfo.m_radiusInSquares
            });
        }

        return new List<ServerClientUtils.SequenceStartData>
        {
            new ServerClientUtils.SequenceStartData(
                useCone
                    ? m_coneSequencePrefab
                    : m_laserSequencePrefab,
                targetPos,
                additionalData.m_abilityResults.HitActorsArray(),
                caster,
                additionalData.m_sequenceSource,
                sequenceParams.ToArray())
        };
    }
}